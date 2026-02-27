namespace MiniERP.Application.Features.Transactions.Queries.GetCashStatement
{
    public sealed record CashStatementResponse(
    Guid TransactionId,
    DateTime Date,
    string Description,
    decimal Debit, // Kasaya Giren
    decimal Credit, // Kasadan Çıkan
    decimal Balance // Kasa Bakiyesi
);
}
