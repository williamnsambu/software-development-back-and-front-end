using System;
using System.Collections.Generic;

namespace DevPulse.Application.Dtos.Dashboard
{
    public class DashboardVm
    {
        public Guid UserId { get; set; }
        public int RepoCount { get; set; }
        public int IssueCount { get; set; }

        public List<PullRequestSummary> PRs { get; set; } = new();
        public List<IssueSummary> Issues { get; set; } = new();

        public double? Weather { get; set; }
    }
}