namespace MiniERP.Application.Features.Transactions.Queries.GetFinancialStatus
{
    public sealed record FinancialStatusResponse(
    List<AccountStatusResponse> CashBalances,
    List<AccountStatusResponse> BankBalances
);
}
