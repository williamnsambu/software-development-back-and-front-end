namespace DevPulse.Application.Dtos.OAuth
{
    public sealed record OAuthCallbackResponse(
        string AccessToken,
        string RefreshToken,
        DateTimeOffset ExpiresAt
    );
}