namespace Challenge04_TenantManagementApi.Models;

public class Group : ISoftDelete
{
    public required string Id { get; set; }

    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public string? MailNickname { get; set; }

    public required DateTimeOffset CreatedDateTime { get; set; }

    public bool IsDeleted { get; set; }
}