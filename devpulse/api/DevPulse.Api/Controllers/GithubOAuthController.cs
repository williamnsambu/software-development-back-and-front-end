using System.Net.Http.Headers;
using System.Text.Json;
using DevPulse.Application.Abstractions;
using DevPulse.Application.Dtos.OAuth;
using DevPulse.Application.Dtos.Providers.Github;
using Microsoft.AspNetCore.Mvc;

namespace DevPulse.Api.Controllers;

[ApiController]
[Route("api/oauth/github")]
public sealed class GithubOAuthController : ControllerBase
{
    private readonly IConfiguration _cfg;
    private readonly IProviderService _providers;
    private readonly IHttpClientFactory _http;
    private readonly IJwtIssuer _jwt;

    public GithubOAuthController(
        IConfiguration cfg,
        IProviderService providers,
        IHttpClientFactory http,
        IJwtIssuer jwt)
    {
        _cfg = cfg;
        _providers = providers;
        _http = http;
        _jwt = jwt;
    }

    // GET /api/oauth/github/start?redirectUri=...&codeChallenge=...
    [HttpGet("start")]
    public IActionResult Start([FromQuery] string redirectUri, [FromQuery] string codeChallenge)
    {
        if (string.IsNullOrWhiteSpace(redirectUri) || string.IsNullOrWhiteSpace(codeChallenge))
            return BadRequest(new { title = "Missing redirectUri or codeChallenge" });

        var clientId = _cfg["GitHub:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
            return Problem("GitHub:ClientId not configured", statusCode: 500);

        var state = Guid.NewGuid().ToString("N");
        var isHttps = Request.IsHttps;

        Response.Cookies.Append("oauth_state", state, new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
            MaxAge = TimeSpan.FromMinutes(10),
            IsEssential = true
        });

        var authorizeUrl =
            "https://github.com/login/oauth/authorize" +
            $"?client_id={Uri.EscapeDataString(clientId!)}" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
            $"&state={Uri.EscapeDataString(state)}" +
            $"&response_type=code" +
            $"&code_challenge={Uri.EscapeDataString(codeChallenge)}" +
            $"&code_challenge_method=S256";

        // If you registered a classic OAuth App and need scopes, you can add:
        // + $"&scope={Uri.EscapeDataString("read:user repo")}"

        return Ok(new { authorizationUrl = authorizeUrl });
    }

    // GET /api/oauth/github/callback?code=...&state=...&redirectUri=...&codeVerifier=...
    [HttpGet("callback")]
    public async Task<IActionResult> Callback(
        [FromQuery] string code,
        [FromQuery] string state,
        [FromQuery] string redirectUri,
        [FromQuery] string codeVerifier)
    {
        // Validate state cookie
        var cookieState = Request.Cookies["oauth_state"];
        var isHttps = Request.IsHttps;

        if (string.IsNullOrWhiteSpace(cookieState) || cookieState != state)
            return BadRequest(new { title = "Invalid OAuth state", status = 400 });

        // delete the cookie with the same flags used when setting it
        Response.Cookies.Delete("oauth_state", new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
            IsEssential = true
        });

        var clientId = _cfg["GitHub:ClientId"];
        var clientSecret = _cfg["GitHub:ClientSecret"];
        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            return Problem("GitHub client settings missing", statusCode: 500);

        var http = _http.CreateClient();
        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var tokenReq = new Dictionary<string, string?>
        {
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["code"] = code,
            ["redirect_uri"] = redirectUri,
            ["grant_type"] = "authorization_code",
            ["code_verifier"] = codeVerifier
        };

        // ---- Exchange code -> tokens (DO NOT throw on non-200) ----
        using var tokenResp = await http.PostAsync(
            "https://github.com/login/oauth/access_token",
            new FormUrlEncodedContent(tokenReq!));

        var tokenJson = await tokenResp.Content.ReadAsStringAsync();

        if (!tokenResp.IsSuccessStatusCode)
        {
            // Bubble up the real GitHub error for visibility (often "bad_verification_code")
            return StatusCode(400, new { title = "GitHub token exchange failed", detail = tokenJson });
        }

        var token = JsonSerializer.Deserialize<GitHubTokenResponse>(tokenJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
            return StatusCode(400, new { title = "Token missing from GitHub response", detail = tokenJson });

        // ---- Fetch GitHub user ----
        var api = _http.CreateClient();
        api.DefaultRequestHeaders.UserAgent.ParseAdd("DevPulse/1.0");
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        using var userResp = await api.GetAsync("https://api.github.com/user");
        var userJson = await userResp.Content.ReadAsStringAsync();

        if (!userResp.IsSuccessStatusCode)
            return StatusCode(400, new { title = "Failed to fetch GitHub user", detail = userJson });

        var ghUser = JsonSerializer.Deserialize<GitHubUserResponse>(userJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (ghUser is null || ghUser.Id == 0)
            return StatusCode(400, new { title = "Malformed GitHub user response", detail = userJson });

        // ---- Upsert connection & issue JWTs ----
        var userInfo = new UserInfoDto(
            Provider: "GitHub",
            ProviderId: ghUser.Id.ToString(),
            Username: ghUser.Login ?? $"user-{ghUser.Id}",
            DisplayName: ghUser.Name,
            Email: ghUser.Email,
            AvatarUrl: ghUser.AvatarUrl
        );

        var ourToken = new OAuthToken(
            AccessToken: token.AccessToken!,
            RefreshToken: token.RefreshToken,
            ExpiresAt: token.ExpiresIn.HasValue
                ? DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn.Value)
                : (DateTimeOffset?)null,
            TokenType: token.TokenType ?? "Bearer"
        );

        var userId = await _providers.UpsertGithubConnectionAsync(userInfo, ourToken);
        var (accessJwt, refreshJwt) = _jwt.IssueFor(userId);

        return Ok(new { accessToken = accessJwt, refreshToken = refreshJwt });
    }
}