using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Products.Commands.CreateProduct
{
    public sealed record CreateProductCommand(
    string Code,
    string Name,
    string Barcode,
    decimal DefaultPrice,
    decimal CriticalStockLevel,
    decimal VatRate,
    Guid CategoryId,
    Guid BrandId,
    Guid UnitId) : IRequest<Result<string>>;
}
