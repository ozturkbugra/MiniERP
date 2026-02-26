namespace MiniERP.Domain.Entities
{
    public sealed class BankTransaction : FinancialTransaction
    {
        public Guid BankId { get; set; }
        public Bank? Bank { get; set; }
    }
}
