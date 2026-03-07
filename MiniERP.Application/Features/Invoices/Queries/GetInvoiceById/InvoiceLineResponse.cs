namespace MiniERP.Application.Features.Invoices.Queries.GetInvoiceById
{
    public sealed record InvoiceLineResponse(
    Guid ProductId,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountRate,
    decimal VatRate,
    decimal LineTotal,
    string WarehouseName
);
}
