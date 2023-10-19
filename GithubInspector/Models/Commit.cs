using System.Text.Json.Serialization;

namespace GithubInspector.Models;

public record struct Commit(
    [property: JsonPropertyName("commit")] Details Details
);