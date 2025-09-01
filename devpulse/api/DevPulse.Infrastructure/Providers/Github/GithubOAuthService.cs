using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DevPulse.Application.Abstractions;
using DevPulse.Application.Dtos.OAuth;
using DevPulse.Application.Options;
using Microsoft.Extensions.Options;
using System.Net.Http;

namespace DevPulse.Infrastructure.Providers.Github;

public sealed class GithubOAuthService : IOAuthService
{
    private readonly GithubOptions _opts;
    private readonly HttpClient _http;

    public GithubOAuthService(IOptions<GithubOptions> opts, IHttpClientFactory httpFactory)
    {
        _opts = opts.Value;
        _http = httpFactory.CreateClient("github");
        _http.BaseAddress ??= new Uri("https://github.com/");
    }

    public string GetAuthorizationUrl(string redirectUri, string codeChallenge)
    {
        var state = Base64Url(RandomBytes(16));
        // we will persist state on the client or session; here we just include it in the URL.
        var url =
            $"https://github.com/login/oauth/authorize" +
            $"?client_id={Uri.EscapeDataString(_opts.ClientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
            $"&scope={Uri.EscapeDataString("repo read:org workflow")}" +
            $"&state={state}" +
            $"&code_challenge={codeChallenge}" +
            $"&code_challenge_method=S256";
        return url;
    }

    public async Task<OAuthToken> ExchangeCodeForTokenAsync(string code, string redirectUri, string codeVerifier)
    {
        // GitHub’s token endpoint (note: still under github.com, not api.github.com)
        using var req = new HttpRequestMessage(HttpMethod.Post, "login/oauth/access_token");
        var body = new Dictionary<string, string>
        {
            ["client_id"] = _opts.ClientId,
            ["client_secret"] = _opts.ClientSecret,   // for public clients you’d use PKCE w/o secret; GitHub accepts both
            ["code"] = code,
            ["redirect_uri"] = redirectUri,
            ["grant_type"] = "authorization_code",
            ["code_verifier"] = codeVerifier
        };
        req.Content = new FormUrlEncodedContent(body);
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var res = await _http.SendAsync(req);
        res.EnsureSuccessStatusCode();

        using var stream = await res.Content.ReadAsStreamAsync();
        var doc = await JsonDocument.ParseAsync(stream);
        var root = doc.RootElement;

        var access = root.GetProperty("access_token").GetString()!;
        var tokenType = root.TryGetProperty("token_type", out var tt) ? tt.GetString() ?? "Bearer" : "Bearer";
        var expiresIn = root.TryGetProperty("expires_in", out var ei) ? (int?)ei.GetInt32() : null;
        var refresh = root.TryGetProperty("refresh_token", out var rf) ? rf.GetString() : null;

        return new OAuthToken(
            AccessToken: access,
            RefreshToken: refresh,
            ExpiresAt: expiresIn.HasValue ? DateTimeOffset.UtcNow.AddSeconds(expiresIn.Value) : null,
            TokenType: tokenType
        );
    }

    public async Task<UserInfoDto> GetUserInfoAsync(string accessToken, string tokenType = "Bearer")
    {
        // GitHub user info is on api.github.com
        using var client = _http; // reuse named client
        client.BaseAddress = new Uri("https://api.github.com/");
        client.DefaultRequestHeaders.UserAgent.ParseAdd("DevPulse/1.0");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(tokenType, accessToken);

        var res = await client.GetAsync("user");
        res.EnsureSuccessStatusCode();

        using var s = await res.Content.ReadAsStreamAsync();
        var doc = await JsonDocument.ParseAsync(s);
        var u = doc.RootElement;

        return new UserInfoDto(
            Provider: "GitHub",
            ProviderId: u.GetProperty("id").GetRawText(),  // id is number; store as string
            Username: u.GetProperty("login").GetString() ?? "",
            DisplayName: u.TryGetProperty("name", out var name) ? name.GetString() : null,
            Email: u.TryGetProperty("email", out var email) ? email.GetString() : null,
            AvatarUrl: u.TryGetProperty("avatar_url", out var av) ? av.GetString() : null
        );
    }

    // Helpers
    private static byte[] RandomBytes(int len) { var b = new byte[len]; RandomNumberGenerator.Fill(b); return b; }
    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+','-').Replace('/','_');
}