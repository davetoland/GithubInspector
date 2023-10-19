using System.Text.Json.Serialization;

namespace GithubInspector.Models;

public record struct Author(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("date")] DateTime Date
);
