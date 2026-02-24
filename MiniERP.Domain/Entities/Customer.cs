using MiniERP.Domain.Common;
using MiniERP.Domain.Enums;

namespace MiniERP.Domain.Entities
{
    public sealed class Customer : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? TaxDepartment { get; set; }
        public string? TaxNumber { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public CustomerType Type { get; set; } = CustomerType.Buyer;

    }
}

