namespace DevPulse.Application.Dtos.Auth
{
    public sealed record TokenPairResponse(string AccessToken, string RefreshToken);
}