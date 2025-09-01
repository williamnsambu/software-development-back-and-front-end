using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using DevPulse.Application.Options;

namespace DevPulse.Infrastructure.Providers.Github
{
    public sealed class GithubWebhookService
    {
        private readonly string _secret;
        private readonly ILogger<GithubWebhookService> _log;

        public GithubWebhookService(IOptions<GithubOptions> opts, ILogger<GithubWebhookService> log)
        {
            _secret = opts.Value.WebhookSecret ?? throw new InvalidOperationException("Missing GitHub:WebhookSecret");
            _log = log;
        }

        /// <summary>Verifies X-Hub-Signature-256 against the raw request body.</summary>
        public bool VerifySignature(string? signatureHeader, string payload)
        {
            if (string.IsNullOrWhiteSpace(signatureHeader)) return false;

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var expected = "sha256=" + Convert.ToHexString(hash).ToLowerInvariant();

            // constant-time compare
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expected),
                Encoding.UTF8.GetBytes(signatureHeader));
        }

        public async Task HandleAsync(string payload, string? eventName)
        {
            // parse minimal fields; route by event
            using var doc = JsonDocument.Parse(payload);
            switch (eventName)
            {
                case "pull_request":
                    await HandlePullRequestAsync(doc.RootElement);
                    break;
                case "push":
                    await HandlePushAsync(doc.RootElement);
                    break;
                default:
                    _log.LogDebug("Unhandled GitHub event: {Event}", eventName);
                    break;
            }
        }

        private Task HandlePullRequestAsync(JsonElement root)
        {
            // map → enqueue/update DB, etc.
            return Task.CompletedTask;
        }

        private Task HandlePushAsync(JsonElement root) => Task.CompletedTask;
    }
}