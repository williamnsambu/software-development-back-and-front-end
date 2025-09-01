using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DevPulse.Application.Abstractions;
using DevPulse.Application.Dtos.Dashboard;
using DevPulse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DevPulse.Infrastructure.Services
{
    public sealed class DashboardService : IDashboardService
    {
        private readonly AppDbContext _db;
        private readonly IWeatherClient _weather;

        public DashboardService(AppDbContext db, IWeatherClient weather)
        {
            _db = db;
            _weather = weather;
        }

        public async Task<DashboardVm> GetDashboardAsync(Guid userId, CancellationToken ct = default)
        {
            // Run sequentially to avoid DbContext concurrency
            var repoCount = await _db.Repositories
                .AsNoTracking()
                .Where(r => r.UserId == userId)
                .CountAsync(ct);

            var issueCount = await _db.Issues
                .AsNoTracking()
                .Where(i => i.UserId == userId || i.Repo!.UserId == userId)
                .CountAsync(ct);

            var prs = await _db.PullRequests
                .AsNoTracking()
                .Where(pr => pr.Repo.UserId == userId)
                .OrderByDescending(pr => pr.UpdatedAt)
                .Take(5)
                .Select(pr => new PullRequestSummary
                {
                    Repo = pr.Repo.Name,
                    Number = pr.Number,
                    Title = pr.Title,
                    State = pr.State,
                    UpdatedAt = pr.UpdatedAt,
                    Url = pr.Url,
                    CiPassing = pr.CiPassing
                })
                .ToListAsync(ct);

            var issues = await _db.Issues
                .AsNoTracking()
                .Where(i => i.UserId == userId || i.Repo!.UserId == userId)
                .OrderByDescending(i => i.UpdatedAt)
                .Take(5)
                .Select(i => new IssueSummary
                {
                    Repo = i.Repo != null ? i.Repo.Name : string.Empty,
                    Title = i.Title,
                    Status = i.Status,
                    UpdatedAt = i.UpdatedAt,
                    Url = i.Url
                })
                .ToListAsync(ct);

            double? tempC = null;
            try
            {
                tempC = await _weather.GetCurrentTempCAsync("San Francisco", "US", ct);
            }
            catch { /* optional: ignore weather errors in demo */ }

            return new DashboardVm
            {
                UserId = userId,
                RepoCount = repoCount,
                IssueCount = issueCount,
                PRs = prs,
                Issues = issues,
                Weather = tempC
            };
        }
    }
}