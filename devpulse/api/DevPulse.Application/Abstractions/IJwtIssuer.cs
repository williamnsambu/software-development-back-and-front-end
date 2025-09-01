using System;

namespace DevPulse.Application.Abstractions
{
    public interface IJwtIssuer
    {
        (string AccessToken, string RefreshToken) IssueFor(Guid userId);
    }
}