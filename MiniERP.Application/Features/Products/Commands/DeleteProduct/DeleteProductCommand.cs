using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Products.Commands.DeleteProduct
{
    public sealed record DeleteProductCommand(Guid Id) : IRequest<Result<string>>;
}
