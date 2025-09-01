using System;
using System.Linq;
using System.Threading.Tasks;
using DevPulse.Application.Abstractions;
using DevPulse.Application.Dtos.OAuth;
using DevPulse.Domain.Entities;
using DevPulse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DevPulse.Infrastructure.Services;

public sealed class ProviderService : IProviderService
{
    private readonly AppDbContext _db;

    public ProviderService(AppDbContext db) => _db = db;

    public async Task<Guid> UpsertGithubConnectionAsync(UserInfoDto user, OAuthToken token)
    {
        var email = string.IsNullOrWhiteSpace(user.Email)
            ? $"{user.Username}@users.noreply.{user.Provider.ToLowerInvariant()}.com"
            : user.Email;

        var localUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (localUser is null)
        {
            localUser = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = "-" // external-account placeholder
            };
            _db.Users.Add(localUser);
            await _db.SaveChangesAsync();
        }

        var conn = await _db.ProviderConnections
            .FirstOrDefaultAsync(c => c.UserId == localUser.Id && c.Provider == "github");

        if (conn is null)
        {
            conn = new ProviderConnection
            {
                Id = Guid.NewGuid(),
                UserId = localUser.Id,
                Provider = "github",
            };
            _db.ProviderConnections.Add(conn);
        }

        // Tokens
        conn.AccessTokenEnc  = token.AccessToken;
        conn.RefreshTokenEnc = token.RefreshToken;
        conn.ExpiresAt       = token.ExpiresAt;

        conn.MetaJson = JsonSerializer.Serialize(new
        {
            provider     = user.Provider,
            providerId   = user.ProviderId,
            username     = user.Username,
            displayName  = user.DisplayName,
            email        = user.Email,
            avatarUrl    = user.AvatarUrl
        });

        await _db.SaveChangesAsync();
        return localUser.Id;
    }

    public async Task ConnectJiraAsync(Guid userId, string email, string apiToken)
    {
        var conn = await _db.ProviderConnections
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Provider == "jira");

        if (conn is null)
        {
            conn = new ProviderConnection
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Provider = "jira",
            };
            _db.ProviderConnections.Add(conn);
        }

        conn.AccessTokenEnc = apiToken;
        conn.MetaJson = JsonSerializer.Serialize(new { email });

        await _db.SaveChangesAsync();
    }

    public async Task DisconnectAsync(Guid userId, string provider)
    {
        var conns = _db.ProviderConnections.Where(c => c.UserId == userId && c.Provider == provider);
        _db.ProviderConnections.RemoveRange(conns);
        await _db.SaveChangesAsync();
    }

    public Task EnqueueSyncAsync(Guid userId)
    {
        // Hook Quartz/Hangfire job here
        return Task.CompletedTask;
    }
}