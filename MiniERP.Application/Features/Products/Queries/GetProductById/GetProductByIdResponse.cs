namespace MiniERP.Application.Features.Products.Queries.GetProductById
{
    public sealed record GetProductByIdResponse(
    Guid Id,
    string Code,
    string Name,
    string Barcode,
    string CategoryName,
    string BrandName,
    string UnitName,
    string WarehouseName,
    decimal DefaultPrice,
    decimal CriticalStockLevel,
    decimal VatRate);
}
