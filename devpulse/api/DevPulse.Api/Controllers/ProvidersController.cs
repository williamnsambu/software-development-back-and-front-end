using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DevPulse.Application.Abstractions;
using DevPulse.Application.Dtos.Providers;
using DevPulse.Api.Extensions;

namespace DevPulse.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/providers")]
public class ProvidersController : ControllerBase
{
    private readonly IProviderService _svc;
    public ProvidersController(IProviderService svc) => _svc = svc;

    [HttpPost("jira/connect")]
    public async Task<IActionResult> ConnectJira([FromBody] JiraConnectRequest req)
    {
        await _svc.ConnectJiraAsync(User.GetUserId(), req.Email, req.ApiToken);
        return NoContent();
    }

    [HttpDelete("{provider}")]
    public async Task<IActionResult> Disconnect(string provider)
    {
        await _svc.DisconnectAsync(User.GetUserId(), provider);
        return NoContent();
    }

    [HttpPost("sync")]
    public async Task<IActionResult> Sync()
    {
        await _svc.EnqueueSyncAsync(User.GetUserId());
        return Accepted();
    }
}