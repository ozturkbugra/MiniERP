namespace MiniERP.Application.Features.Cashs.Queries.GetAllCashes
{
    public sealed record CashResponse(
    Guid Id,
    string Name,
    string CurrencyType,
    string? CreatedByName,
    string? UpdatedByName);
}
