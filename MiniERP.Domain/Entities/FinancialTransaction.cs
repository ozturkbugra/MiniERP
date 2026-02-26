using MiniERP.Domain.Common;

namespace MiniERP.Domain.Entities
{
    public abstract class FinancialTransaction : BaseEntity
    {
        public decimal Debit { get; set; }  // Borç Girişi
        public decimal Credit { get; set; } // Alacak Girişi
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }

        public void Validate()
        {
            if (Debit > 0 && Credit > 0)
                throw new Exception("Bir hareket aynı anda hem Borç (Debit) hem Alacak (Credit) olamaz.");

            if (Debit == 0 && Credit == 0)
                throw new Exception("Borç veya Alacak alanlarından en az biri sıfırdan büyük olmalıdır.");
        }
    }
}
