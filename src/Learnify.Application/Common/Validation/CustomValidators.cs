using FluentValidation;
using FluentValidation.Validators;
using System.Text.RegularExpressions;

namespace Learnify.Application.Common.Validation;

public static class CustomValidators
{
    public static IRuleBuilderOptions<T, string> Password<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
                .WithMessage("Password is required")
                .WithDescription("The password field was empty or not provided")
            .MinimumLength(ValidationConstants.Password.MinLength)
                .WithMessage($"Password must be at least {ValidationConstants.Password.MinLength} characters")
                .WithDescription($"The password provided is shorter than the minimum of {ValidationConstants.Password.MinLength} characters")
            .MaximumLength(ValidationConstants.Password.MaxLength)
                .WithMessage($"Password cannot exceed {ValidationConstants.Password.MaxLength} characters")
                .WithDescription($"The password provided exceeds the maximum of {ValidationConstants.Password.MaxLength} characters")
            .Must(ContainUppercase)
                .WithMessage("Password must contain at least one uppercase letter")
                .WithDescription("The password must include at least one uppercase letter (A-Z)")
            .Must(ContainLowercase)
                .WithMessage("Password must contain at least one lowercase letter")
                .WithDescription("The password must include at least one lowercase letter (a-z)")
            .Must(ContainDigit)
                .WithMessage("Password must contain at least one digit")
                .WithDescription("The password must include at least one digit (0-9)")
            .Must(ContainSpecialChar)
                .WithMessage("Password must contain at least one special character (@$!%*?&_-#)")
                .WithDescription("The password must include at least one special character from: @$!%*?&_-#")
            .Must(NotContainWhitespace)
                .WithMessage("Password cannot contain spaces")
                .WithDescription("The password contains spaces which are not allowed");
    }

    public static IRuleBuilderOptions<T, string> Username<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
                .WithMessage("Username is required")
                .WithDescription("The username field was empty or not provided")
            .MinimumLength(ValidationConstants.Username.MinLength)
                .WithMessage($"Username must be at least {ValidationConstants.Username.MinLength} characters")
                .WithDescription((_, val) => $"The username '{TruncateValue(val, 20)}' is shorter than the minimum of {ValidationConstants.Username.MinLength} characters")
            .MaximumLength(ValidationConstants.Username.MaxLength)
                .WithMessage($"Username cannot exceed {ValidationConstants.Username.MaxLength} characters")
                .WithDescription((_, val) => $"The username '{TruncateValue(val, 20)}' exceeds the maximum of {ValidationConstants.Username.MaxLength} characters")
            .Matches(ValidationConstants.Username.Pattern)
                .WithMessage(ValidationConstants.Username.FormatMessage)
                .WithDescription((_, val) => $"The username '{val}' must start with a letter and contain only letters, numbers, dots, or underscores")
            .Must(NotBeReservedUsername)
                .WithMessage("This username is reserved and cannot be used")
                .WithDescription((_, val) => $"The username '{val}' is reserved for system use and cannot be registered")
            .Must(NotContainConsecutivePeriods)
                .WithMessage("Username cannot contain consecutive periods")
                .WithDescription((_, val) => $"The username '{val}' contains consecutive periods (..) which are not allowed")
            .Must(NotEndWithPeriodOrUnderscore)
                .WithMessage("Username cannot end with a period or underscore")
                .WithDescription((_, val) => $"The username '{val}' cannot end with a period or underscore");
    }

    public static IRuleBuilderOptions<T, string> EmailAddress<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
                .WithMessage("Email is required")
                .WithDescription((_, val) => "The email field was empty or not provided")
            .MaximumLength(ValidationConstants.Email.MaxLength)
                .WithMessage($"Email cannot exceed {ValidationConstants.Email.MaxLength} characters")
                .WithDescription((_, val) => $"The email '{TruncateValue(val, 30)}' exceeds the maximum length of {ValidationConstants.Email.MaxLength} characters")
            .Matches(ValidationConstants.Email.Pattern)
                .WithMessage("Please enter a valid email address")
                .WithDescription((_, val) => $"The value '{val}' is not a valid email format")
            .Must(NotBeDisposableEmail)
                .WithMessage("Disposable email addresses are not allowed")
                .WithDescription((_, val) => $"The email domain in '{val}' is a disposable email provider which is not allowed");
    }

    public static IRuleBuilderOptions<T, string> PersonName<T>(this IRuleBuilder<T, string> ruleBuilder, string fieldName)
    {
        return ruleBuilder
            .MaximumLength(ValidationConstants.Name.MaxLength)
                .WithMessage($"{fieldName} cannot exceed {ValidationConstants.Name.MaxLength} characters")
            .Matches(ValidationConstants.Name.Pattern)
                .When(x => !string.IsNullOrEmpty(x as string))
                .WithMessage($"{fieldName} can only contain letters, spaces, hyphens, and apostrophes");
    }

    #region Private Helpers

    private static bool ContainUppercase(string password) =>
        !string.IsNullOrEmpty(password) && password.Any(char.IsUpper);

    private static bool ContainLowercase(string password) =>
        !string.IsNullOrEmpty(password) && password.Any(char.IsLower);

    private static bool ContainDigit(string password) =>
        !string.IsNullOrEmpty(password) && password.Any(char.IsDigit);

    private static bool ContainSpecialChar(string password) =>
        !string.IsNullOrEmpty(password) && password.Any(c => "@$!%*?&_-#".Contains(c));

    private static bool NotContainWhitespace(string password) =>
        string.IsNullOrEmpty(password) || !password.Any(char.IsWhiteSpace);

    private static bool NotBeReservedUsername(string username) =>
        string.IsNullOrEmpty(username) ||
        !ValidationConstants.Username.ReservedUsernames.Contains(username.ToLowerInvariant());

    private static bool NotContainConsecutivePeriods(string username) =>
        string.IsNullOrEmpty(username) || !username.Contains("..");

    private static bool NotEndWithPeriodOrUnderscore(string username) =>
        string.IsNullOrEmpty(username) ||
        (!username.EndsWith('.') && !username.EndsWith('_'));

    private static readonly HashSet<string> DisposableEmailDomains = new(StringComparer.OrdinalIgnoreCase)
    {
        "tempmail.com", "throwaway.com", "guerrillamail.com", "10minutemail.com",
        "mailinator.com", "sharklasers.com", "trash-mail.com", "fakeinbox.com",
        "getnada.com", "tempail.com", "mohmal.com", "discard.email"
    };

    private static bool NotBeDisposableEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) return true;
        var domain = email.Split('@').LastOrDefault();
        return domain == null || !DisposableEmailDomains.Contains(domain);
    }

    /// <summary>
    /// Truncates a value for display in descriptions to avoid overly long messages
    /// </summary>
    private static string TruncateValue(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return "(empty)";
        if (value.Length <= maxLength) return value;
        return value[..maxLength] + "...";
    }

    #endregion
}