namespace MiniERP.Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 1,    // Beklemede (Taslak, henüz rezerve işlemi yok)
        Approved = 2,   // Onaylandı (Miktar formülle dinamik rezerve sayılacak)
        Invoiced = 3,   // Faturalandı (Rezerve biter, ilerleyen modülde fiziki stoktan düşer)
        Canceled = 4    // İptal Edildi (Rezerve boşa çıkar)
    }
}
