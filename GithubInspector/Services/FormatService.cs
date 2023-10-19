using GithubInspector.Models;

namespace GithubInspector.Services;

public class OutputFormatter
{
    public OutputFormatter() { }

    public List<string> FormatCommits(List<Commit> commits)
    {
        var formatted = commits.Select((x, i) => FormatCommit(x.Details, i, commits.Count)).ToList();
        formatted.Insert(0, $"Showing last {commits.Count} commits, most recent first:");
        return formatted;

        static string FormatCommit(Details commit, int index, int count)
        {
            return $"[{count - index}] Author: {commit.Author.Name} ({commit.Author.Email}) | {commit.Author.Date:U} | {commit.Message}";
        }
    }
}
