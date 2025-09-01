namespace DevPulse.Application.Dtos.Dashboard
{
    public sealed class DashboardVm
    {
        public required IReadOnlyList<PullRequestVm> PRs { get; init; }
        public required IReadOnlyList<IssueVm> Issues { get; init; }
        public required WeatherVm Weather { get; init; }
    }

    public sealed record PullRequestVm(Guid Id, string Title, string Url, bool CiPassing);

    public sealed record IssueVm(Guid Id, string Title, string Url, string Status);

    public sealed record WeatherVm(double Temp, string Description);
}