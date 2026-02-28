using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Brands.Commands.UpdateBrand
{
    public sealed record UpdateBrandCommand(Guid Id, string Name) : IRequest<Result<string>>;
}
