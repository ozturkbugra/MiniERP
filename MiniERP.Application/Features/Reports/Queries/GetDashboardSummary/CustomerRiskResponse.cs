namespace MiniERP.Application.Features.Reports.Queries.GetDashboardSummary
{
    public sealed record CustomerRiskResponse(
    Guid CustomerId,
    string CustomerName,
    decimal Balance
);
}
