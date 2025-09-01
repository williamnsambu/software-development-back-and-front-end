using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DevPulse.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DevPulse.Infrastructure.Security
{
    public sealed class JwtIssuer : IJwtIssuer
    {
        private readonly IConfiguration _cfg;

        public JwtIssuer(IConfiguration cfg)
        {
            _cfg = cfg;
        }

        private static byte[] GetSigningKeyBytes(IConfiguration cfg)
        {
            var b64 = cfg["Auth:SigningKeyBase64"];
            if (!string.IsNullOrWhiteSpace(b64))
            {
                try { return Convert.FromBase64String(b64); }
                catch (FormatException)
                {
                    throw new InvalidOperationException("Auth:SigningKeyBase64 is not valid Base64.");
                }
            }

            var raw = cfg["Auth:SigningKey"]
                      ?? throw new InvalidOperationException("Missing Auth:SigningKey or Auth:SigningKeyBase64.");
            var bytes = Encoding.UTF8.GetBytes(raw);
            if (bytes.Length < 32)
                throw new InvalidOperationException("Signing key must be at least 32 bytes (256 bits).");
            return bytes;
        }

        public (string AccessToken, string RefreshToken) IssueFor(Guid userId)
        {
            var issuer   = _cfg["Auth:Issuer"]   ?? "DevPulse";
            var audience = _cfg["Auth:Audience"] ?? "DevPulseClient";

            var keyBytes = GetSigningKeyBytes(_cfg);
            var creds = new SigningCredentials(
                new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256);

            var now = DateTimeOffset.UtcNow;
            var accessTokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = issuer,
                Audience = audience,
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                }),
                NotBefore = now.UtcDateTime,
                Expires = now.AddHours(1).UtcDateTime,
                SigningCredentials = creds
            };

            var handler = new JwtSecurityTokenHandler();
            var accessToken = handler.WriteToken(handler.CreateToken(accessTokenDescriptor));

            // TODO: replace with real refresh token persistence/rotation
            var refreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            // IMPORTANT: element names must match the interface exactly
            return (AccessToken: accessToken, RefreshToken: refreshToken);
        }
    }
}