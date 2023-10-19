using MediatR;
using GithubInspector.Requests;
using GithubInspector.Queries;

namespace GithubInspector.Endpoints;

public static class GithubEndpoint
{
    public static void MapGithubEndpoint(this WebApplication app)
    {
        app.MapGet("/api/v1/{owner}/{repo}/contributors", GetCommits);
    }

    private static async Task<IResult> GetCommits(
        [AsParameters] GithubRequest request, 
        IMediator mediator,
        CancellationToken cancel)
    {
        var validation = await mediator.Send(new ValidateRequestQuery(request), cancel);
        if (!validation.IsValid)
            return Results.BadRequest(validation.Errors.Select(x => x.ErrorMessage));

        var response = await mediator.Send(new GetCommitsQuery(request), cancel);
        if (response.IsFailed)
            return Results.NotFound(response.Reasons.Single().Message);

        var commits = response.Value;
        if (!commits.Any())
            return Results.NoContent();

        var formatted = await mediator.Send(new FormatCommitsQuery(commits), cancel);

        return Results.Ok(formatted);
    }        
}
