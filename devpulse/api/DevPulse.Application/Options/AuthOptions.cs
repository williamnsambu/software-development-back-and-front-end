namespace DevPulse.Application.Options
{
    public sealed class AuthOptions
    {
        public string Issuer { get; init; } = "";
        public string Audience { get; init; } = "";
        public string SigningKey { get; init; } = "";
    }
}