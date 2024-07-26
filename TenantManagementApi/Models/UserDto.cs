using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TenantManagementApi.Models;

public record UserDto
{
    [JsonProperty(PropertyName = "id")]
    public required string Id { get; init; }

    [Required]
    [JsonProperty(PropertyName = "displayName")]
    public string? DisplayName { get; init; }

    [Required]
    [JsonProperty(PropertyName = "userPrincipalName")]
    public string? UserPrincipalName { get; init; }

    [Required]
    [JsonProperty(PropertyName = "mailNickname")]
    public string? MailNickname { get; init; }

    [Required]
    [JsonProperty(PropertyName = "createdDateTime")]
    public DateTimeOffset CreatedDateTime { get; init; }
}
