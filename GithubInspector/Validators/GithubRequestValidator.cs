using FluentValidation;
using GithubInspector.Requests;

namespace GithubInspector.Validators;

public class GithubRequestValidator : AbstractValidator<GithubRequest>
{
    public GithubRequestValidator()
    {
        RuleFor(x => x.Owner).IsValidString();
        RuleFor(x => x.Repo).IsValidString();
    }
}

public static class AbstractValidatorExtensions
{
    public static void IsValidString<GithubRequest>(this IRuleBuilderInitial<GithubRequest, string> builder)
        => builder
        .NotNull()
        .NotEmpty()
        .MinimumLength(1);
}
