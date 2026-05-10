namespace MiniERP.Application.Features.Dashboard.Queries.GetDashboardSummary
{
    public sealed record RecentActivityResponse(DateTime Date, string Description, string Type, string StatusColor);
}
