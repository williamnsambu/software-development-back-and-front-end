namespace DevPulse.Application.Dtos.Providers.Github
{
    public sealed record GitHubUserResponse
    {
        public long Id { get; init; }
        public string? Login { get; init; }
        public string? Name { get; init; }
        public string? Email { get; init; }
        public string? AvatarUrl { get; init; }
    }
}