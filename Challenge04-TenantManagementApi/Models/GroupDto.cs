using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Challenge04_TenantManagementApi.Models;

public record GroupDto
{
    [JsonProperty(PropertyName = "id")]
    public required string Id { get; init; }

    [Required]
    [JsonProperty(PropertyName = "displayName")]
    public string? DisplayName { get; init; }

    [Required]
    [JsonProperty(PropertyName = "description")]
    public string? Description { get; init; }

    [Required]
    [JsonProperty(PropertyName = "mailNickname")]
    public string? MailNickname { get; init; }
}
