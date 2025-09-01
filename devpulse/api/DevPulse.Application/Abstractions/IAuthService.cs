using DevPulse.Application.Dtos.Auth;

namespace DevPulse.Application.Abstractions
{
    public interface IAuthService
    {
        Task<TokenPairResponse> RegisterAsync(string email, string password);
        Task<TokenPairResponse> LoginAsync(string email, string password);
        Task<TokenPairResponse> RefreshAsync(string refreshToken);
    }
}