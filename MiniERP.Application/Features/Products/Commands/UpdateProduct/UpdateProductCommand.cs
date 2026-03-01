using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Products.Commands.UpdateProduct
{
    public sealed record UpdateProductCommand(
    Guid Id,
    string Code,
    string Name,
    string Barcode,
    decimal DefaultPrice,
    decimal CriticalStockLevel,
    decimal VatRate,
    Guid CategoryId,
    Guid BrandId,
    Guid UnitId,
    Guid WarehouseId) : IRequest<Result<string>>;
}
