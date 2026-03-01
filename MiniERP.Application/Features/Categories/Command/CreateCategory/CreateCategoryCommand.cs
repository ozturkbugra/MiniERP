using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Categories.Command.CreateCategory
{
    public sealed record CreateCategoryCommand(string Name, string? Description) : IRequest<Result<string>>;
}
