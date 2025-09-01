using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DevPulse.Infrastructure.Providers.Github;

namespace DevPulse.Api.Controllers;

[ApiController]
[Route("webhooks")]
[Produces("application/json")]
public class WebhooksController : ControllerBase
{
    private readonly GithubWebhookService _svc;
    public WebhooksController(GithubWebhookService svc) => _svc = svc;

    [HttpPost("github")]
    public async Task<IActionResult> Github()
    {
        // GitHub sends: X-Hub-Signature-256: sha256=<hex>
        var signature = Request.Headers["X-Hub-Signature-256"].ToString();
        if (signature.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase))
            signature = signature.Substring("sha256=".Length);

        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync();

        if (!_svc.VerifySignature(signature, payload))
            return Unauthorized();

        var eventName = Request.Headers["X-GitHub-Event"].ToString();
        await _svc.HandleAsync(payload, eventName);

        return Ok();
    }
}