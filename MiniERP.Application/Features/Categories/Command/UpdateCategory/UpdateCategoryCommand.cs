using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Categories.Command.UpdateCategory
{
    public sealed record UpdateCategoryCommand(Guid Id, string Name, string? Description) : IRequest<Result<string>>;
}
