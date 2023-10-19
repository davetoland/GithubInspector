using GithubInspector.Queries;
using GithubInspector.Services;
using MediatR;

namespace GithubInspector.Handlers;

public class FormatOutputQueryHandler : IRequestHandler<FormatCommitsQuery, List<string>>
{
    private readonly OutputFormatter _formatter;

    public FormatOutputQueryHandler(OutputFormatter formatter)
    {
        _formatter = formatter;
    }

    public Task<List<string>> Handle(FormatCommitsQuery query, CancellationToken cancellationToken)
    {
        return Task.FromResult(_formatter.FormatCommits(query.Commits));
    }
}