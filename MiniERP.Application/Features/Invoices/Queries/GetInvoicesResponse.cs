namespace MiniERP.Application.Features.Invoices.Queries
{
    public sealed record GetInvoicesResponse(
    Guid Id,
    string InvoiceNumber,
    DateTime InvoiceDate,
    Guid CustomerId,
   string CustomerName,
    Guid? WarehouseId,  
    string WarehouseName, 
    decimal GrandTotal,   
    string Status,        
    string CreatedByName,
    string? UpdatedByName
);
}
