namespace MiniERP.Application.Features.Cashs.Queries.GetCashById
{
    public sealed record CashByIdResponse(
    Guid Id,
    string Name,
    string CurrencyType,
    string? CreatedByName,
    string? UpdatedByName);
}
