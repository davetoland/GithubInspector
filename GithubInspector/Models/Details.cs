using System.Text.Json.Serialization;

namespace GithubInspector.Models;

public record struct Details(
    [property: JsonPropertyName("author")] Author Author,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("url")] string Url
);
