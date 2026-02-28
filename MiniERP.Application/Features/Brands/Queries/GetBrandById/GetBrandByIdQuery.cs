using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Brands.Queries.GetBrandById
{
    public sealed record GetBrandByIdQuery(Guid Id) : IRequest<Result<GetBrandByIdResponse>>;
}
