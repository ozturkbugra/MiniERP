namespace MiniERP.Domain.Enums
{
    public enum TransactionType
    {
        SalesInvoice = 1,      // Satış Faturası
        PurchaseInvoice = 2,   // Alış Faturası
        Collection = 3,        // Tahsilat (Müşteriden bize)
        Payment = 4,           // Ödeme (Bizden tedarikçiye)
        CashEntry = 5,         // Kasaya nakit girişi
        CashExit = 6,          // Kasadan nakit çıkışı
        BankTransfer = 7,      // Banka transferi
        OpeningBalance = 8     // Devir / Açılış Fişi
    }
}
