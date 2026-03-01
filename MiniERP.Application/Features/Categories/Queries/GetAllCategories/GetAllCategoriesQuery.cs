using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Categories.Queries.GetAllCategories
{
    public sealed record GetAllCategoriesQuery() : IRequest<Result<List<GetAllCategoriesResponse>>>;
}
