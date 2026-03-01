using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Categories.Queries.GetCategoryById
{
    public sealed record GetCategoryByIdQuery(Guid Id) : IRequest<Result<GetCategoryByIdResponse>>;
}
