using MediatR;
using GithubInspector.Requests;
using GithubInspector.Queries;

namespace GithubInspector.Endpoints;

public static class GithubEndpoint
{
    public static void MapGithubEndpoint(this WebApplication app)
    {
        app.MapGet("/api/v1/{owner}/{repo}/contributors", GetContributors);
    }

    public static async Task<IResult> GetContributors([AsParameters] GithubRequest request, 
        IMediator mediator,
        CancellationToken cancel)
    {
        var validation = await mediator.Send(new ValidateRequestQuery(request), cancel);
        if (!validation.IsValid)
            return Results.BadRequest(validation.Errors.Select(x => x.ErrorMessage));

        var result = await mediator.Send(new GetCommitsQuery(request), cancel);
        if (result.IsFailed)
            return Results.NotFound(result.Reasons.Single().Message);

        var contributors = result.Value;
        if (!contributors.Any())
            return Results.NoContent();

        var formatted = await mediator.Send(new FormatCommitsQuery(contributors), cancel);

        return Results.Ok(formatted);
    }        
}
