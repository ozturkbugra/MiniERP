using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Dashboard.Queries.GetDashboardSummary
{
    public sealed record GetDashboardSummaryQuery : IRequest<Result<GetDashboardSummaryResponse>>;
}
