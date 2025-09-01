using System;
using System.Security.Cryptography;
using System.Text;

namespace DevPulse.Infrastructure.Security
{
    public static class HmacSignatureVerifier
    {
        public static bool VerifySha256(string secret, string signatureHeader, string payload, string prefix = "sha256=")
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var expected = prefix + Convert.ToHexString(hash).ToLowerInvariant();
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expected),
                Encoding.UTF8.GetBytes(signatureHeader));
        }
    }
}