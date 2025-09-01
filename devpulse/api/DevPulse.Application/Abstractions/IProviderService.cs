using System;
using System.Threading.Tasks;
using DevPulse.Application.Dtos.OAuth;

namespace DevPulse.Application.Abstractions
{
    public interface IProviderService
    {
        Task ConnectJiraAsync(Guid userId, string email, string apiToken);
        Task DisconnectAsync(Guid userId, string provider);
        Task EnqueueSyncAsync(Guid userId);
        Task<Guid> UpsertGithubConnectionAsync(object userInfo, OAuthToken token);
    }

    // Assuming OAuthToken is defined elsewhere in your project
}