using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Brands.Commands.CreateBrand
{
    public sealed record CreateBrandCommand(string Name) : IRequest<Result<string>>;
}
