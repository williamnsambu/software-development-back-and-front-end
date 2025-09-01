using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DevPulse.Domain.Entities;

namespace DevPulse.Application.Abstractions
{
    /// <summary>
    /// Minimal Jira client abstraction.
    /// Many Jira APIs use basic auth (email + API token) and a JQL query.
    /// </summary>
    public interface IJiraClient
    {
        /// <param name="baseUrl">e.g. https://yourcompany.atlassian.net</param>
        /// <param name="email">Jira account email</param>
        /// <param name="apiToken">Jira API token</param>
        /// <param name="jql">Jira Query Language string</param>
        Task<IReadOnlyList<Issue>> GetIssuesAsync(
            string baseUrl,
            string email,
            string apiToken,
            string jql,
            int maxResults = 50,
            CancellationToken ct = default);
    }
}