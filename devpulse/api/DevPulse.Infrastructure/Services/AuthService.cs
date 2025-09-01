using System;
using System.Threading.Tasks;
using DevPulse.Application.Abstractions;
using DevPulse.Domain.Entities;
using DevPulse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DevPulse.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtIssuer _jwt;

    public AuthService(AppDbContext db, IJwtIssuer jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<(string AccessToken, string RefreshToken)> RegisterAsync(string email, string password)
    {
        // Enforce unique emails (simple demo check)
        var exists = await _db.Users.AnyAsync(u => u.Email == email);
        if (exists) throw new InvalidOperationException("Email already registered.");

        var user = new User
        {
            Email = email,
            // BCrypt hashing (BCrypt.Net-Next package)
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return _jwt.IssueFor(user.Id);
    }

    public async Task<(string AccessToken, string RefreshToken)> LoginAsync(string email, string password)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        return _jwt.IssueFor(user.Id);
    }

    public Task<(string AccessToken, string RefreshToken)> RefreshAsync(string refreshToken)
    {
        // TODO: add a persistent refresh-token store / rotation
        throw new NotImplementedException();
    }
}