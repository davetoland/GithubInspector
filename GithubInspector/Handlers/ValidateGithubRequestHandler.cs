using FluentValidation.Results;
using GithubInspector.Queries;
using GithubInspector.Validators;
using MediatR;

namespace GithubInspector.Handlers;

public class ValidateGithubRequestHandler : IRequestHandler<ValidateRequestQuery, ValidationResult>
{
    private readonly GithubRequestValidator _validator;

    public ValidateGithubRequestHandler(GithubRequestValidator validator)
    {
        _validator = validator;
    }

    public Task<ValidationResult> Handle(ValidateRequestQuery query, CancellationToken cancellationToken)
    {
        return _validator.ValidateAsync(query.Request, cancellationToken);
    }
}
