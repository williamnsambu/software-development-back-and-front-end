using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DevPulse.Application.Abstractions;
using DevPulse.Application.Dtos.Auth;

namespace DevPulse.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenPairResponse>> Register([FromBody] RegisterRequest req)
        => Ok(await _auth.RegisterAsync(req.Email, req.Password));

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenPairResponse>> Login([FromBody] LoginRequest req)
        => Ok(await _auth.LoginAsync(req.Email, req.Password));

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenPairResponse>> Refresh([FromBody] string refreshToken)
        => Ok(await _auth.RefreshAsync(refreshToken));
}