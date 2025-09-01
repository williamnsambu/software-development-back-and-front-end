using System;

namespace DevPulse.Application.Dtos.Dashboard
{
    public class IssueSummary
    {
        public string Repo { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset UpdatedAt { get; set; }
        public string Url { get; set; } = string.Empty;
    }
}