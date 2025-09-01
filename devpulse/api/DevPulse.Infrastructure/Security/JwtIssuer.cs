using System;
using DevPulse.Application.Abstractions;

namespace DevPulse.Infrastructure.Security
{
    public sealed class JwtIssuer : IJwtIssuer
    {
        public (string accessToken, string refreshToken) IssueFor(Guid userId)
        {
            // TODO: real JWT issuing using AuthOptions/signing key in cfg.
            throw new NotImplementedException();
        }
    }
}