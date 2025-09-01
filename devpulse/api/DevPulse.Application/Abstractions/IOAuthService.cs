using DevPulse.Application.Dtos.OAuth;

namespace DevPulse.Application.Abstractions;

public interface IOAuthService
{
    // Build the user-facing authorize URL (PKCE uses a code_challenge you generate in the SPA)
    string GetAuthorizationUrl(string redirectUri, string codeChallenge);

    // After callback: exchange code -> access/refresh token
    Task<OAuthToken> ExchangeCodeForTokenAsync(string code, string redirectUri, string codeVerifier);

    // Fetch normalized user info from the provider using the access token
    Task<UserInfoDto> GetUserInfoAsync(string accessToken, string tokenType = "Bearer");
}