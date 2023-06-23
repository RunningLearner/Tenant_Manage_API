using System.Security.Cryptography.X509Certificates;
using Microsoft.Graph;
using Azure.Identity;
using Challenge04_TenantManagementApi.Services;
using Microsoft.OpenApi.Models;
using Challenge04_TenantManagementApi.Filters;
using Challenge04_TenantManagementApi.Middlewares;
using Challenge04_TenantManagementApi.Data;
using Microsoft.EntityFrameworkCore;

namespace Challenge04_TenantManagementApi;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
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
                Description = "미리 약속한 API Key값을 입력합니다. 이 값은 HyundaiM365Api 제품의 환경설정에서 설정합니다."
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

        ValidateConfigurations(out string? tenantId, out string? clientId, out string? certFilePath);
        var clientCertificate = new X509Certificate2(certFilePath!, string.Empty, X509KeyStorageFlags.MachineKeySet);
        var clientCertCredential = new ClientCertificateCredential(tenantId, clientId, clientCertificate);

        // Graph API 등록
        var graphServiceClient = new GraphServiceClient(clientCertCredential);
        services.AddSingleton(graphServiceClient);

        // 주기적으로 작동하는 data fetcher 등록
        services.AddHostedService<TimedDataFetchingService>();

        services.AddScoped<UserService>();
        services.AddScoped<GroupService>();
    }

    private void ValidateConfigurations(out string? tenantId, out string? clientId, out string? certFilePath)
    {
        tenantId = _configuration["AzureAd:TenantId"];
        clientId = _configuration["AzureAd:ClientId"];
        certFilePath = _configuration["AzureAd:CertFile"];

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

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHealthChecks("/health");
            endpoints.MapControllers();
        });
    }
}