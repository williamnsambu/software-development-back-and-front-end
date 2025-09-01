using System;

namespace DevPulse.Application.Dtos.Dashboard
{
    public class PullRequestSummary
    {
        public string Repo { get; set; } = string.Empty;
        public int Number { get; set; }
        public string Title { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty; // open/closed/merged
        public DateTimeOffset UpdatedAt { get; set; }
        public string Url { get; set; } = string.Empty;
        public bool CiPassing { get; set; }
    }
}