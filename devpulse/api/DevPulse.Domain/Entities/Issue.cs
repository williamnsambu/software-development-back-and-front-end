using System;

namespace DevPulse.Domain.Entities
{
    public class Issue
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? RepoId { get; set; }              // nullable for Jira-only items
        public Repository? Repo { get; set; }

        public Guid? UserId { get; set; }              // owner/assignee in our system
        public User? User { get; set; }

        public string ProviderIssueId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public DateTimeOffset UpdatedAt { get; set; }
    }
}