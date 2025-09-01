namespace DevPulse.Application.Dtos.OAuth;

public sealed record UserInfoDto(
    string Provider,      // e.g. "GitHub"
    string ProviderId,    // provider's user id
    string Username,      // e.g. "octocat"
    string? DisplayName,  // e.g. "Monalisa Octocat"
    string? Email,
    string? AvatarUrl
);