namespace Challenge04_TenantManagementApi.Models;

public class Group
{
    public required string Id { get; init; }

    public string? DisplayName { get; init; }

    public string? Description { get; init; }

    public string? MailNickname { get; init; }
}