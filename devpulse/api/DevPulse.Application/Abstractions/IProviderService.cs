using DevPulse.Application.Dtos.OAuth;

namespace DevPulse.Application.Abstractions;

public interface IProviderService
{
    Task<Guid> UpsertGithubConnectionAsync(UserInfoDto user, OAuthToken token);
    Task ConnectJiraAsync(Guid userId, string email, string apiToken);
    Task DisconnectAsync(Guid userId, string provider);
    Task EnqueueSyncAsync(Guid userId);
}