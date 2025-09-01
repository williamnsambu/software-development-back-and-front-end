using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DevPulse.Application.Abstractions;

namespace DevPulse.Api.Controllers;

[ApiController]
[Route("api/oauth/github")]
public class GithubOAuthController : ControllerBase
{
    private readonly IOAuthService _oauth;
    private readonly IProviderService _providers;
    private readonly IJwtIssuer _jwt;
    private readonly ILogger<GithubOAuthController> _log;

    public GithubOAuthController(
        IOAuthService oauth,
        IProviderService providers,
        IJwtIssuer jwt,
        ILogger<GithubOAuthController> log)
    {
        _oauth = oauth; _providers = providers; _jwt = jwt; _log = log;
    }

    // SPA sends redirectUri + PKCE code_challenge (SPA keeps code_verifier)
    [HttpGet("start")]
    [AllowAnonymous]
    public IActionResult Start([FromQuery] string redirectUri, [FromQuery] string codeChallenge)
    {
        if (string.IsNullOrWhiteSpace(redirectUri) || string.IsNullOrWhiteSpace(codeChallenge))
            return BadRequest(new ProblemDetails { Title = "Missing redirectUri or codeChallenge" });

        // generate anti-CSRF state nonce, store as secure cookie, return auth URL containing same state
        var state = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                        .TrimEnd('=').Replace('+','-').Replace('/','_');

        Response.Cookies.Append("oauth_state", state, new CookieOptions {
            HttpOnly = true, Secure = true, SameSite = SameSiteMode.None, // allow cross-site OAuth redirect
            MaxAge = TimeSpan.FromMinutes(10), IsEssential = true
        });

        var url = _oauth.GetAuthorizationUrl(redirectUri, codeChallenge)
                        // if your IOAuthService doesn’t add state itself, append here:
                        + $"&state={Uri.EscapeDataString(state)}";

        return Ok(new { authorizationUrl = url });
    }

    // Callback handler: SPA calls this with code + redirectUri + code_verifier (+ state passthrough)
    [HttpGet("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> Callback(
        [FromQuery] string code,
        [FromQuery] string redirectUri,
        [FromQuery] string codeVerifier,
        [FromQuery] string? state)
    {
        if (string.IsNullOrWhiteSpace(code) ||
            string.IsNullOrWhiteSpace(redirectUri) ||
            string.IsNullOrWhiteSpace(codeVerifier))
            return BadRequest(new ProblemDetails { Title = "Missing code, redirectUri or codeVerifier" });

        // validate state to prevent CSRF
        var cookieState = Request.Cookies["oauth_state"];
        if (string.IsNullOrEmpty(state) || string.IsNullOrEmpty(cookieState) || !string.Equals(state, cookieState, StringComparison.Ordinal))
            return BadRequest(new ProblemDetails { Title = "Invalid OAuth state" });

        // clear the cookie
        Response.Cookies.Delete("oauth_state", new CookieOptions { Secure = true, SameSite = SameSiteMode.None });

        try
        {
            var token = await _oauth.ExchangeCodeForTokenAsync(code, redirectUri, codeVerifier);
            var user  = await _oauth.GetUserInfoAsync(token.AccessToken, token.TokenType);

            var userId = await _providers.UpsertGithubConnectionAsync(user, token);
            var (access, refresh) = _jwt.IssueFor(userId);

            // redirect SPA with first-party tokens (or return JSON if you prefer)
            return Redirect($"/oauth/success#access={access}&refresh={refresh}");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "GitHub OAuth callback failed");
            return Problem(title: "OAuth exchange failed", statusCode: 502);
        }
    }
}