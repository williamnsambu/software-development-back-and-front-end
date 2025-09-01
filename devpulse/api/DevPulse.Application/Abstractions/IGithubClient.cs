using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DevPulse.Domain.Entities;

namespace DevPulse.Application.Abstractions
{
    /// <summary>
    /// Lightweight GitHub client abstraction used by application services.
    /// Auth is handled elsewhere (OAuth); pass the provider access token.
    /// </summary>
    public interface IGithubClient
    {
        Task<IReadOnlyList<Repository>> GetRepositoriesAsync(
            string accessToken,
            CancellationToken ct = default);

        Task<IReadOnlyList<PullRequest>> GetPullRequestsAsync(
            string accessToken,
            string owner,
            string repo,
            CancellationToken ct = default);

        Task<IReadOnlyList<Issue>> GetIssuesAsync(
            string accessToken,
            string owner,
            string repo,
            CancellationToken ct = default);
    }
}