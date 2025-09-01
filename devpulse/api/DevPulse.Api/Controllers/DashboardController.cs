using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DevPulse.Application.Abstractions;
using DevPulse.Application.Dtos.Dashboard;
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
    public async Task<ActionResult<DashboardVm>> Get()
        => Ok(await _svc.GetDashboardAsync(User.GetUserId()));
}