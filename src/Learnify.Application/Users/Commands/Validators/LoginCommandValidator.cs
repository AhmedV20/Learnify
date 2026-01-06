using FluentValidation;
using Learnify.Application.Common.Validation;
using Learnify.Application.Features.Authentication.Commands;

namespace Learnify.Application.Users.Commands.Validators;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}