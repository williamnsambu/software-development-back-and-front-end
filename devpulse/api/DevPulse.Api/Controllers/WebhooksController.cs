using Microsoft.AspNetCore.Mvc;
using DevPulse.Infrastructure.Providers.Github;

namespace DevPulse.Api.Controllers;

[ApiController]
[Route("webhooks")]
public class WebhooksController : ControllerBase
{
    private readonly GithubWebhookService _svc;
    public WebhooksController(GithubWebhookService svc) => _svc = svc;

    [HttpPost("github")]
    public async Task<IActionResult> Github()
    {
        var signature = Request.Headers["X-Hub-Signature-256"].ToString();
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync();
        if (!_svc.VerifySignature(signature, payload)) return Unauthorized();
        await _svc.HandleAsync(payload, Request.Headers["X-GitHub-Event"].ToString());
        return Ok();
    }
}