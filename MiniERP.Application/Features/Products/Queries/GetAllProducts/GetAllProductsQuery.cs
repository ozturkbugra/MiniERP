using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Products.Queries.GetAllProducts
{
    public sealed record GetAllProductsQuery() : IRequest<Result<List<GetAllProductsResponse>>>;
}
