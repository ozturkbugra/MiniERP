using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Products.Queries.GetProductById
{
    public sealed record GetProductByIdQuery(Guid Id) : IRequest<Result<GetProductByIdResponse>>;
}
