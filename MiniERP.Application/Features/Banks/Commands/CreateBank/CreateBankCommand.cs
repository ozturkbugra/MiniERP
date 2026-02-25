using MediatR;
using MiniERP.Domain.Common;
using MiniERP.Domain.Enums;

namespace MiniERP.Application.Features.Banks.Commands.CreateBank
{
    public sealed record CreateBankCommand(
    string BankName,
    string AccountName,
    string IBAN,
    string? BranchName,
    CurrencyType CurrencyType) : IRequest<Result<string>>;
}
