using Proj3.Domain.Entities.Authentication;

namespace Proj3.Application.Services.Authentication.Result;

public record AuthenticationResult(
    User user,
    string AccessToken,
    string RefreshToken
);