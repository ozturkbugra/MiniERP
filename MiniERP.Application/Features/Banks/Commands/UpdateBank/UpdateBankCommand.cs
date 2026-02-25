using MediatR;
using MiniERP.Domain.Common;
using MiniERP.Domain.Enums;

namespace MiniERP.Application.Features.Banks.Commands.UpdateBank
{
    public sealed record UpdateBankCommand(
     Guid Id,
     string BankName,
     string AccountName,
     string IBAN,
     string? BranchName,
     CurrencyType CurrencyType) : IRequest<Result<string>>;
}
