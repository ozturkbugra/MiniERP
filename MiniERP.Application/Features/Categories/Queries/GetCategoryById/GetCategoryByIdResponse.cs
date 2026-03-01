namespace MiniERP.Application.Features.Categories.Queries.GetCategoryById
{
    public sealed record GetCategoryByIdResponse(Guid Id, string Name, string? Description);
}
