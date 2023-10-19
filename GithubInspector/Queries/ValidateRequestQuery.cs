using MediatR;
using GithubInspector.Requests;
using FluentValidation.Results;

namespace GithubInspector.Queries;

public class ValidateRequestQuery : IRequest<ValidationResult>
{
    public GithubRequest Request { get; }

    public ValidateRequestQuery(GithubRequest request)
    {
        Request = request;
    }
}
