using Challenge04_TenantManagementApi.Models;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using User = Microsoft.Graph.Models.User;

namespace Challenge04_TenantManagementApi.Services;

public sealed class UserService
{
    private readonly ILogger<UserService> _logger;
    private readonly GraphServiceClient _graphClient;

    public UserService(GraphServiceClient graphClient, ILogger<UserService> logger)
    {
        _logger = logger;
        _graphClient = graphClient;
    }

    public async Task<UserDto> GetAsync(string id)
    {
        var user = await _graphClient.Users[id].GetAsync();
        _logger.LogInformation("UserId : {Id}", user?.Id);

        return ItemToDto(user!);
    }

    public async Task<UserDto> AddAsync(CreateUserDto createUserDto)
    {
        var user = new User
        {
            AccountEnabled = true,
            DisplayName = createUserDto.DisplayName,
            MailNickname = createUserDto.MailNickname,
            UserPrincipalName = createUserDto.UserPrincipalName,
            PasswordProfile = new PasswordProfile
            {
                ForceChangePasswordNextSignIn = true,
                Password = Guid.NewGuid().ToString(),
            },
        };

        var createdUser = await _graphClient.Users.PostAsync(user);
        _logger.LogInformation("NewUser : {@NewUser}", createdUser);

        return ItemToDto(createdUser!);
    }

    public async Task UpdateAsync(string id, UserDto userDto)
    {
        var user = new User
        {
            DisplayName = userDto.DisplayName,
            MailNickname = userDto.MailNickname,
            UserPrincipalName = userDto.UserPrincipalName,
        };

        await _graphClient.Users[id].PatchAsync(user);
        _logger.LogInformation("UpdatedUser : {@User}", user);
    }

    public async Task DeleteAsync(string id)
    {
        var user = await _graphClient.Users[id].GetAsync();
        await _graphClient.Users[id].DeleteAsync();
        _logger.LogInformation("DeletedUser : {@DeletedUser}", user);
    }

    private static UserDto ItemToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id!,
            DisplayName = user.DisplayName,
            UserPrincipalName = user.UserPrincipalName,
            MailNickname = user.MailNickname
        };
    }
}