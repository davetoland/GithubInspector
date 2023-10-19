using MediatR;
using GithubInspector.Models;

namespace GithubInspector.Queries;

public class FormatCommitsQuery : IRequest<List<string>>
{
    public List<Commit> Commits { get; }

    public FormatCommitsQuery(List<Commit> commits)
    {
        Commits = commits;
    }
}