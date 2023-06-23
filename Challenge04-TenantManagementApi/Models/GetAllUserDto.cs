using System.ComponentModel.DataAnnotations;

namespace Challenge04_TenantManagementApi.Models;

public record GetAllUserDto
{
    [Range(10, 50)]
    public int PageSize { get; set; } = 10;

    public string? NextUrl { get; set; }
}