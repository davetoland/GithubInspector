using MediatR;
using FluentResults;
using GithubInspector.Models;
using GithubInspector.Requests;

namespace GithubInspector.Queries;

public class GetCommitsQuery : IRequest<Result<List<Commit>>>
{
    public GithubRequest Request { get; }

    public GetCommitsQuery(GithubRequest request)
    {
        Request = request;
    }
}
