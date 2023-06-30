using System.Security.Cryptography.X509Certificates;
using Microsoft.Graph;
using Azure.Identity;
using Challenge04_TenantManagementApi.Services;
using Microsoft.OpenApi.Models;
using Challenge04_TenantManagementApi.Filters;
using Challenge04_TenantManagementApi.Middlewares;
using Challenge04_TenantManagementApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Challenge04_TenantManagementApi;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// 런타입에 호출되어 서비스를 컨테이너에 등록합니다.
    /// </summary>
    /// <param name="services">의존성 주입에 사용될 컨테이너</param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<HealthCheckService>("HealthCheckService"); ;

        services.AddDbContext<GraphDbContext>(opt =>
            opt.UseSqlite(_configuration.GetConnectionString("DefaultConnection")));

        services.AddMvc(x =>
        {
            x.Filters.AddService<ValidateModelFilter>();
        }).ConfigureApiBehaviorOptions(x =>
        {
            x.SuppressModelStateInvalidFilter = true;
        });

        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

        services.AddScoped(x =>
        {
            var actionContext = x.GetRequiredService<IActionContextAccessor>().ActionContext;
            var factory = x.GetRequiredService<IUrlHelperFactory>();
            return factory.GetUrlHelper(actionContext!);
        });

        services.AddScoped<ValidateModelFilter>();
        services.AddSingleton<ExceptionHandlingMiddleware>();
        services.AddHttpContextAccessor();
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            // HTTP 메소드에 따라 정렬 로직 추가
            c.OrderActionsBy((apiDesc) =>
            {
                var httpMethodPriority = new List<string> { "GET", "POST", "PUT", "DELETE" };
                var httpMethod = apiDesc.HttpMethod ?? "GET";
                var index = httpMethodPriority.IndexOf(httpMethod);
                if (index == -1)
                {
                    index = int.MaxValue; // If the method is not in the list, put it at the end.
                }
                return $"{index}:{apiDesc.RelativePath}";
            });

            // Swagger 웹 클라이언트 화면에 인증 버튼을 추가한다.
            // 참고: https://swagger.io/docs/specification/authentication/api-keys/
            c.AddSecurityDefinition("AuthByApiKeyHeader", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey, // 인증 형태
                In = ParameterLocation.Header, // 인증 정보가 추가될 위치
                Name = "X-API-KEY", // 여기서는 HTTP Header의 Key값, 규격상 Authorization 헤더는 쓰면 안 된다
                Description = "미리 약속한 API Key값을 입력합니다. 이 값은 appsettings.json 에서 설정합니다."
            });

            // 인증 버튼에서 입력받은 인증 정보를 Swagger에서 보내는 실제 HTTP 요청에 적용한다.
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Type = SecuritySchemeType.ApiKey,
                            In = ParameterLocation.Header,
                            Name = "X-API-KEY",
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "AuthByApiKeyHeader" // 위의 인증 버튼에서 지정한 이름을 넣어, 입력받은 인증값을 연동한다
                            }
                        },
                        new List<string>()
                    }
                });
        });

        GraphServiceClient graphServiceClient = InitGraphClient();
        services.AddSingleton(graphServiceClient);

        // 주기적으로 작동하는 data fetcher 등록
        services.AddHostedService<TimedBackgroundService>();
        services.AddScoped<DataFetchingService>();
        services.AddScoped<UserService>();
        services.AddScoped<GroupService>();
    }

    private GraphServiceClient InitGraphClient()
    {
        var tenantId = _configuration["AzureAd:TenantId"];
        var clientId = _configuration["AzureAd:ClientId"];
        var certFilePath = _configuration["AzureAd:CertFile"];
        ValidateConfigurations(tenantId, clientId, certFilePath);

        var clientCertificate = new X509Certificate2(certFilePath!, string.Empty, X509KeyStorageFlags.MachineKeySet);
        var clientCertCredential = new ClientCertificateCredential(tenantId, clientId, clientCertificate);

        // Graph API를 사용하기 위한 SDK 초기화
        return new GraphServiceClient(clientCertCredential);
    }

    private static void ValidateConfigurations(string? tenantId, string? clientId, string? certFilePath)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentNullException(nameof(tenantId), "설정 파일 내 tenantId의 값이 비어있습니다.");
        }

        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new ArgumentNullException(nameof(clientId), "설정 파일 내 clientId의 값이 비어있습니다.");
        }

        if (string.IsNullOrWhiteSpace(certFilePath))
        {
            throw new ArgumentNullException(nameof(certFilePath), "설정 파일 내 certFilePath의 값이 비어있습니다.");
        }

        if (!File.Exists(certFilePath))
        {
            throw new FileNotFoundException("인증에 필요한 pfx 파일을 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 런타임에 호출되어 미들웨어들의 순서를 정하거나 설정을 한다.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="env"></param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
            EnsureDatabaseUpdated(app.ApplicationServices);
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHealthChecks("/health");
            endpoints.MapControllers();
        });
    }

    private static void EnsureDatabaseUpdated(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GraphDbContext>();
        // 애플리케이션 시작 시 데이터베이스에 대해 보류 중인 마이그레이션을 적용
        context.Database.Migrate();
    }
}