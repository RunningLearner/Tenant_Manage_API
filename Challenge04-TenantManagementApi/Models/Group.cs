namespace Challenge04_TenantManagementApi.Models;

public class Group
{
    public required string Id { get; set; }

    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public string? MailNickname { get; set; }
}