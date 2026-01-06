using FluentValidation;
using Learnify.Application.Common.Validation;
using Learnify.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Learnify.Application.Users.Commands.Validators;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RegisterUserCommandValidator(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;

        RuleFor(x => x.Email)
            .EmailAddress()
            .MustAsync(BeUniqueEmail)
                .WithMessage("An account with this email already exists");

        RuleFor(x => x.UserName)
            .Username()
            .MustAsync(BeUniqueUsername)
                .WithMessage("This username is already taken");

        RuleFor(x => x.Password)
            .Password();

        RuleFor(x => x.FirstName)
            .PersonName("First name");

        RuleFor(x => x.LastName)
            .PersonName("Last name");
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(email)) return true;
        var existingUser = await _userManager.FindByEmailAsync(email);
        return existingUser == null;
    }

    private async Task<bool> BeUniqueUsername(string username, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(username)) return true;
        var existingUser = await _userManager.FindByNameAsync(username);
        return existingUser == null;
    }
}