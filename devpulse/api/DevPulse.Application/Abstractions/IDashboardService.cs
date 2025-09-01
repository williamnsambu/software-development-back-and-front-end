using System;
using System.Threading;
using System.Threading.Tasks;
using DevPulse.Application.Dtos.Dashboard;

namespace DevPulse.Application.Abstractions
{
    public interface IDashboardService
    {
        Task<DashboardVm> GetDashboardAsync(Guid userId, CancellationToken ct = default);
    }
}