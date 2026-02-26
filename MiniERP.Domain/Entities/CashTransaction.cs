namespace MiniERP.Domain.Entities
{
    public sealed class CashTransaction : FinancialTransaction
    {
        public Guid CashId { get; set; }
        public Cash? Cash { get; set; }
    }
}
