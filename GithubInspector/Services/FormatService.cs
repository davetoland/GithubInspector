using GithubInspector.Models;

namespace GithubInspector.Services;

public class OutputFormatter
{
    public static List<string> FormatCommits(IEnumerable<Commit> commits)
        => commits.Select(x => 
            $"[{x.Details.Author.Date:yyyy-MM-dd}] " +
            $"{x.Details.Author.Name} | " + // ({x.Details.Author.Email}): " +
            $"{(x.Details.Message.Length > 120 ? x.Details.Message[..117]+"..." : x.Details.Message)}").ToList();
}
