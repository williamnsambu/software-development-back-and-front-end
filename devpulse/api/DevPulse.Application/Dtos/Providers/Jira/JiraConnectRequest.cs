namespace DevPulse.Application.Dtos.Providers;

public sealed class JiraConnectRequest
{
    public string Email { get; init; } = string.Empty;
    public string ApiToken { get; init; } = string.Empty;
}