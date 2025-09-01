using DevPulse.Application.Dtos.Auth;

namespace DevPulse.Application.Abstractions
{
    public interface IAuthService
    {
        Task<(string AccessToken, string RefreshToken)> RegisterAsync(string email, string password);
        Task<(string AccessToken, string RefreshToken)> LoginAsync(string email, string password);
        Task<(string AccessToken, string RefreshToken)> RefreshAsync(string refreshToken);
    }
}