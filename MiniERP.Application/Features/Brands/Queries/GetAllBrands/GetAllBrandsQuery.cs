using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Brands.Queries.GetAllBrands
{
    public sealed record GetAllBrandsQuery() : IRequest<Result<List<GetAllBrandsResponse>>>;
}
