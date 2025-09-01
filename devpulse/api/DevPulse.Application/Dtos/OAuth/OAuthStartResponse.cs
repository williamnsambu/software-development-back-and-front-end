namespace DevPulse.Application.Dtos.OAuth
{
    public sealed record OAuthStartResponse(
        string AuthorizationUrl,
        string State
    );
}