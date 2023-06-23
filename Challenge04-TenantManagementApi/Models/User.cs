namespace Challenge04_TenantManagementApi.Models;

public class User
{
    public required string Id { get; init; }

    public string? DisplayName { get; init; }

    public string? UserPrincipalName { get; init; }

    public string? MailNickname { get; init; }
}
