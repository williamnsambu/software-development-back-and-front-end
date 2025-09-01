using DevPulse.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DevPulse.Api.Extensions;

namespace DevPulse.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _svc;
    public DashboardController(IDashboardService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = User.GetUserId(); // from ClaimsPrincipalExtensions
        return Ok(await _svc.GetDashboardAsync(userId));
    }
}