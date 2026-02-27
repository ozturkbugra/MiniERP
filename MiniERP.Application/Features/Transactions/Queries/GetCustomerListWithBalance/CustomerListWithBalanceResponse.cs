namespace MiniERP.Application.Features.Transactions.Queries.GetCustomerListWithBalance
{
    public sealed record CustomerListWithBalanceResponse(
    Guid Id,
    string Name,
    decimal TotalDebit,
    decimal TotalCredit,
    decimal Balance
);
}
