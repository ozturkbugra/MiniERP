namespace MiniERP.Application.Features.Reports.Queries.GetDashboardSummary
{
    public sealed record DashboardSummaryResponse(
     decimal DailyTurnover,
     List<CustomerRiskResponse> TopDebtors
 );
}
