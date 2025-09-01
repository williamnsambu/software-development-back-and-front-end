namespace DevPulse.Application.Options
{
    public sealed class GithubOptions
    {
        public string ClientId { get; init; } = "";
        public string ClientSecret { get; init; } = "";
        public string WebhookSecret { get; init; } = "";
    }
}