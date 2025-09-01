using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DevPulse.Application.Abstractions;

namespace DevPulse.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    // Minimal request/response contracts for this controller
    public sealed record RegisterRequest(string Email, string Password);
    public sealed record LoginRequest(string Email, string Password);
    public sealed record TokenPairResponse(string AccessToken, string RefreshToken);

    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenPairResponse>> Register([FromBody] RegisterRequest req)
    {
        var (access, refresh) = await _auth.RegisterAsync(req.Email, req.Password);
        return Ok(new TokenPairResponse(access, refresh));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenPairResponse>> Login([FromBody] LoginRequest req)
    {
        var (access, refresh) = await _auth.LoginAsync(req.Email, req.Password);
        return Ok(new TokenPairResponse(access, refresh));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenPairResponse>> Refresh([FromBody] string refreshToken)
    {
        var (access, refresh) = await _auth.RefreshAsync(refreshToken);
        return Ok(new TokenPairResponse(access, refresh));
    }
}