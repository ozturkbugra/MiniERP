namespace MiniERP.Application.Features.Products.Queries.GetProductLedger
{
    public sealed record ProductLedgerResponse(
        DateTime Date,
        string FirmName,
        string TransactionType,
        decimal Quantity,         // İşlem Miktarı (+ Giriş, - Çıkış)
        decimal UnitPrice,        // Birim Fiyat
        decimal LineTotal,        // 🚀 Satır Toplam Tutarı (Miktar * Fiyat)
        decimal RunningQuantity,  // 🚀 Kalan Miktar (O anki depo mevcudu)
        decimal RunningValue,     // 🚀 Kalan Tutar (O anki stokun parasal değeri)
        string DocumentNo
    );
}
