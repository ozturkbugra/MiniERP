namespace MiniERP.Application.Features.Products.Queries.GetAllProducts
{
    public sealed record GetAllProductsResponse(
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
