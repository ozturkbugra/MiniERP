using MiniERP.Domain.Common;
using MiniERP.Domain.Enums;

namespace MiniERP.Domain.Entities
{
    public sealed class Bank : BaseEntity
    {
        public string BankName { get; set; } = string.Empty;    
        public string AccountName { get; set; } = string.Empty; // Mevduat Hesabı
        public string IBAN { get; set; } = string.Empty;
        public string? BranchName { get; set; } // Şube Adı
        public CurrencyType CurrencyType { get; set; }
    }
}
