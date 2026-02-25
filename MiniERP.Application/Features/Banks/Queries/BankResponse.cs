namespace MiniERP.Application.Features.Banks.Queries
{
    public sealed record BankResponse(
     Guid Id,
     string BankName,
     string AccountName,
     string IBAN,
     string? BranchName,
     string CurrencyType,
     string? CreatedByName,
     string? UpdatedByName
     );
}
