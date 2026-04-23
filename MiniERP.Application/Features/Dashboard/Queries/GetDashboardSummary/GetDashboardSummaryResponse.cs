using MiniERP.Application.Features.Reports.Queries.GetDashboardSummary;
using MiniERP.Application.Features.StockTransactions.Queries.GetCriticalStock;

namespace MiniERP.Application.Features.Dashboard.Queries.GetDashboardSummary
{
    public sealed record GetDashboardSummaryResponse
    {
        public decimal TotalSales { get; init; }
        public decimal TotalStockValue { get; init; }
        public decimal TotalCashBankBalance { get; init; }

        // 📈 Grafikler
        public List<MonthlySalesResponse> MonthlySales { get; init; } = new();

        // 🕒 Son İşlemler
        public List<RecentActivityResponse> RecentActivities { get; init; } = new();


        public List<CustomerRiskResponse> TopDebtors { get; init; } = new();
    }
}

