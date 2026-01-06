using MediatR;
using Learnify.Application.Features.Authentication.Commands;

namespace Learnify.Application.Users.Commands.GoogleAuth;

/// <summary>
/// Command for Google OAuth sign-in
/// </summary>
public class GoogleLoginCommand : IRequest<LoginResponse>
{
    /// <summary>
    /// Google ID token received from frontend
    /// </summary>
    public string IdToken { get; set; } = string.Empty;
}
