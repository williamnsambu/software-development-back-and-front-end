using DevPulse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using Microsoft.Extensions.DependencyInjection;

namespace DevPulse.Infrastructure.Jobs;

public sealed class SyncJob : IJob
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<SyncJob> _log;

    public SyncJob(IServiceProvider sp, ILogger<SyncJob> log)
    {
        _sp = sp;
        _log = log;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Example: iterate users that have provider connections
        var userIds = await db.ProviderConnections
            .Select(pc => pc.UserId)
            .Distinct()
            .ToListAsync(context.CancellationToken);

        _log.LogInformation("SyncJob running for {Count} users", userIds.Count);

        // TODO: call your provider clients for each user and upsert PRs/issues.
        // Keep it idempotent (unique indices already help, e.g. PR RepoId+Number).
    }
}