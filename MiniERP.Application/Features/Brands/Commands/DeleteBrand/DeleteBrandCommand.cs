using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Brands.Commands.DeleteBrand
{
    public sealed record DeleteBrandCommand(Guid Id) : IRequest<Result<string>>;
}
