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
            // counts
            var repoCountTask = _db.Repositories
                .Where(r => r.UserId == userId)
                .CountAsync(ct);

            var issueCountTask = _db.Issues
                .Where(i => i.UserId == userId || i.Repo!.UserId == userId)
                .CountAsync(ct);

            // latest 5 PRs for user’s repos
            var prsTask = _db.PullRequests
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

            // latest 5 issues for user’s repos or user
            var issuesTask = _db.Issues
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

            // optional weather (safe to swallow errors in demo if provider unset)
            double? tempC = null;
            try
            {
                tempC = await _weather.GetCurrentTempCAsync("San Francisco", "US", ct);
            }
            catch
            {
                // ignore if provider not configured
            }

            await Task.WhenAll(repoCountTask, issueCountTask, prsTask, issuesTask);

            return new DashboardVm
            {
                UserId = userId,
                RepoCount = repoCountTask.Result,
                IssueCount = issueCountTask.Result,
                PRs = prsTask.Result,
                Issues = issuesTask.Result,
                Weather = tempC
            };
        }
    }
}