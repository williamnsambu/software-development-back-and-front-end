namespace DevPulse.Application.Dtos.OAuth;

public sealed record OAuthToken(
    string AccessToken,
    string? RefreshToken,
    DateTimeOffset? ExpiresAt,
    string TokenType = "Bearer"
);