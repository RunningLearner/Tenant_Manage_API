namespace Challenge04_TenantManagementApi.Models;

public interface ISoftDelete
{
    public bool IsDeleted { get; set; }
}
