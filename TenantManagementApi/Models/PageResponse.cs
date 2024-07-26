namespace TenantManagementApi.Models;

public class PageResponse<T>
{
    public List<T>? Data { get; set; }

    public string? NextUrl { get; set; }
}
