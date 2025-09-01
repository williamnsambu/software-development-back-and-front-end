using System;

namespace DevPulse.Domain.Entities
{
    public class Repository
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string ProviderRepoId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;

        public ICollection<PullRequest> PullRequests { get; set; } = new List<PullRequest>();
        public ICollection<Issue> Issues { get; set; } = new List<Issue>();
    }
}