namespace MiniERP.Domain.Enums
{
    public enum InvoiceType
    {
        Purchase = 1, // Satınalma (Tedarikçiden mal girişi)
        Sales = 2,   // Satış (Müşteriye mal çıkışı)
        PurchaseReturn = 3, // Alım İade Faturası (Stok çıkar - Aldığımızı geri yolluyoruz)
        SalesReturn = 4     // Satış İade Faturası (Stok girer - Sattığımız ürün geri geliyor)
    }
}
