namespace MiniERP.Application.Features.Transactions.Queries.GetBankStatement
{
    public sealed record BankStatementResponse(
    Guid TransactionId,
    DateTime Date,
    string Description,
    decimal Debit, // Giren Para
    decimal Credit, // Çıkan Para
    decimal Balance // O anki Banka Bakiyesi
);
}
