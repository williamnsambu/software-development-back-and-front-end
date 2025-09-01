namespace DevPulse.Application.Dtos.Providers.Github
{
    public sealed record GitHubTokenResponse
    {
        public string? AccessToken { get; init; }
        public string? TokenType { get; init; }
        public string? Scope { get; init; }
        public int? ExpiresIn { get; init; }
        public string? RefreshToken { get; init; }
        public int? RefreshTokenExpiresIn { get; init; }
    }
}