using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Challenge04_TenantManagementApi.Models;

public record CreateUserDto
{
    [Required]
    [JsonProperty(PropertyName = "displayName")]
    public string? DisplayName { get; init; }

    [Required]
    [JsonProperty(PropertyName = "userPrincipalName")]
    public string? UserPrincipalName { get; init; }

    [Required]
    [JsonProperty(PropertyName = "mailNickname")]
    public string? MailNickname { get; init; }
}
