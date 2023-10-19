using MediatR;
using FluentResults;
using GithubInspector.Models;
using GithubInspector.Queries;
using GithubInspector.Services;

namespace GithubInspector.Handlers;

public class GithubCommitsRequestHandler : IRequestHandler<GetCommitsQuery, Result<List<Commit>>>
{
    private readonly GithubService _githubService;

    public GithubCommitsRequestHandler(GithubService githubService)
    {
        _githubService = githubService;
    }

    // public Task<Result<List<GithubContributor>>> Handle(GetContributorsQuery query, CancellationToken cancel)
    // {
    //     return _githubService.GetContributors(query.Request, cancel);
    // }

    public Task<Result<List<Commit>>> Handle(GetCommitsQuery query, CancellationToken cancel)
    {
        return _githubService.GetCommits(query.Request, cancel);
    }
}
