using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Reports.Queries.GetDashboardSummary
{
    public sealed record GetDashboardSummaryQuery() : IRequest<Result<DashboardSummaryResponse>>;
}
