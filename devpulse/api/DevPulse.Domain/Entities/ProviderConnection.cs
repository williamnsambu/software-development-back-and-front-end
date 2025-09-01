using System;

namespace DevPulse.Domain.Entities
{
    public class ProviderConnection
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }                // <-- single FK
        public string Provider { get; set; } = "";
        public string AccessTokenEnc { get; set; } = "";
        public string? RefreshTokenEnc { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }
        public string? MetaJson { get; set; }

        public User User { get; set; } = null!;         // <-- single nav
    }
}