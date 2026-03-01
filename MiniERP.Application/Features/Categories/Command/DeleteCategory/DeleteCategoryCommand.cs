using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Categories.Command.DeleteCategory
{
    public sealed record DeleteCategoryCommand(Guid Id) : IRequest<Result<string>>;
}
