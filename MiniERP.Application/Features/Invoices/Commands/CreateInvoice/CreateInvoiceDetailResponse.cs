namespace MiniERP.Application.Features.Invoices.Commands.CreateInvoice
{
    public sealed record CreateInvoiceDetailResponse(
    Guid ProductId,
    Guid WarehouseId,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountRate,
    decimal VatRate
);
}
