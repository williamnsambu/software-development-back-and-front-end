using System;

namespace DevPulse.Domain.Entities
{
    public class PullRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid RepoId { get; set; }
        public Repository Repo { get; set; } = null!;

        public int Number { get; set; }
        public string Title { get; set; } = string.Empty;
        public string State { get; set; } = "open";
        public DateTimeOffset UpdatedAt { get; set; }
        public string Author { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public bool CiPassing { get; set; }
    }
}