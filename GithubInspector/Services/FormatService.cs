using GithubInspector.Models;

namespace GithubInspector.Services;

public class OutputFormatter
{
    public OutputFormatter() { }

    public List<string> FormatCommits(List<Commit> commits)
    {
        var sample = commits.Select((x, i) => FormatCommit(x.Details, i, commits.Count)).ToList();
        sample.Insert(0, $"Showing last {commits.Count} commits, most recent first:");
        return sample;

        static string FormatCommit(Details commit, int index, int count)
        {
            return $"[{count - index}] Author: {commit.Author.Name} ({commit.Author.Email}) | {commit.Author.Date:U} | {commit.Message}";
        }
    }
}
