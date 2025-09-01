using System;

namespace DevPulse.Application.Abstractions
{
    public interface IJwtIssuer
    {
        (string accessToken, string refreshToken) IssueFor(Guid userId);
    }
}