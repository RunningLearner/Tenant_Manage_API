namespace TenantManagementApi.Models;

public interface ISoftDelete
{
    public bool IsDeleted { get; set; }
}
