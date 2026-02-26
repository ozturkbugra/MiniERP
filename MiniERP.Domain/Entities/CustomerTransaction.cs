namespace MiniERP.Domain.Entities
{
    public sealed class CustomerTransaction : FinancialTransaction
    {
        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }
    }
}
