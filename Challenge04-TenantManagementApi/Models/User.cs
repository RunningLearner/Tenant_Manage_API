namespace Challenge04_TenantManagementApi.Models;

public class User
{
    public required string Id { get; set; }

    public string? DisplayName { get; set; }

    public string? UserPrincipalName { get; set; }

    public string? MailNickname { get; set; }

    public required DateTimeOffset CreatedDateTime { get; set; }
}
