using DevPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevPulse.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSets
    public DbSet<User> Users => Set<User>();
    public DbSet<ProviderConnection> ProviderConnections => Set<ProviderConnection>();
    public DbSet<Repository> Repositories => Set<Repository>();
    public DbSet<PullRequest> PullRequests => Set<PullRequest>();
    public DbSet<Issue> Issues => Set<Issue>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // User
        b.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).HasMaxLength(256).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();

            e.HasMany<ProviderConnection>()
            .WithOne(pc => pc.User)
            .HasForeignKey(pc => pc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

            e.HasMany<Repository>()
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

            e.HasMany<Issue>() // user’s “global” issues (e.g., Jira)
            .WithOne(i => i.User)
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.NoAction);
        });

        // ProviderConnection
        b.Entity<ProviderConnection>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Provider).HasMaxLength(64).IsRequired();
        });

        // Repository
        b.Entity<Repository>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ProviderRepoId).HasMaxLength(128).IsRequired();
            e.Property(x => x.Name).HasMaxLength(256).IsRequired();
            e.Property(x => x.Owner).HasMaxLength(256).IsRequired();

            // NOTE: pair PullRequests with PullRequest.Repo
            e.HasMany(r => r.PullRequests)
            .WithOne(pr => pr.Repo)
            .HasForeignKey(pr => pr.RepoId)
            .OnDelete(DeleteBehavior.Cascade);

            // Pair Issues with Issue.Repo
            e.HasMany(r => r.Issues)
            .WithOne(i => i.Repo)
            .HasForeignKey(i => i.RepoId)
            .OnDelete(DeleteBehavior.SetNull);
        });

        // PullRequest
        b.Entity<PullRequest>(e =>
        {
            e.HasKey(pr => pr.Id);

            e.Property(pr => pr.Title).HasMaxLength(512).IsRequired();
            e.Property(pr => pr.State).HasMaxLength(64).IsRequired();
            e.Property(pr => pr.Url).HasMaxLength(2048).IsRequired();
            e.Property(pr => pr.Author).HasMaxLength(256).IsRequired();

            e.HasIndex(pr => new { pr.RepoId, pr.Number }).IsUnique();
        });

        // Issue
        b.Entity<Issue>(e =>
        {
            e.HasKey(i => i.Id);

            e.Property(i => i.ProviderIssueId).HasMaxLength(128).IsRequired();
            e.Property(i => i.Title).HasMaxLength(512).IsRequired();
            e.Property(i => i.Status).HasMaxLength(64).IsRequired();
            e.Property(i => i.Url).HasMaxLength(2048).IsRequired();
        });
    }
}