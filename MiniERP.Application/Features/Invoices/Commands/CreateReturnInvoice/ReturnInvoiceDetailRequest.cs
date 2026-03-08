namespace MiniERP.Application.Features.Invoices.Commands.CreateReturnInvoice
{
    public sealed record ReturnInvoiceDetailRequest(
        Guid ProductId,
        Guid WarehouseId,
        decimal ReturnQuantity,
        decimal UnitPrice,
        decimal DiscountRate,
        decimal VatRate
    );
}
