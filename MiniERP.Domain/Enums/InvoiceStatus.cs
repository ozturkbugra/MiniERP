namespace MiniERP.Domain.Enums
{
    public enum InvoiceStatus
    {
        Draft = 1,    // Taslak (Üzerinde değişiklik yapılabilir, entegrasyon çalışmaz)
        Approved = 2, // Onaylandı (Stok düştü, cari işlendi, değiştirilemez!)
        Canceled = 3  // İptal Edildi (Ters hareket atılarak sıfırlandı)
    }
}
