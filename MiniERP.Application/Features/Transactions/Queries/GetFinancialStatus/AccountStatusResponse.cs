namespace MiniERP.Application.Features.Transactions.Queries.GetFinancialStatus
{
    public sealed record AccountStatusResponse(Guid Id, string Name, decimal Balance);
}
