namespace MiniERP.Application.Features.Invoices.Queries.GetInvoiceById
{
    public sealed record GetInvoiceByIdResponse(
    Guid Id,
    string InvoiceNumber,
    DateTime InvoiceDate,
    string CustomerName,
    string WarehouseName,
    decimal TotalGross,
    decimal TotalDiscount,
    decimal TotalVat,
    decimal GrandTotal,
    string Status,
    string CreatedByName,
    List<InvoiceLineResponse> Lines
);
}
