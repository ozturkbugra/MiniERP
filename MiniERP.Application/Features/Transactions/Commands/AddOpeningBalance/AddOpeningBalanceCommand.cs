using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Transactions.Commands.AddOpeningBalance
{
    public sealed record AddOpeningBalanceCommand(
    DateTime Date,
    string Description,
    decimal Amount,
    Guid? CashId,
    Guid? BankId,
    Guid? CustomerId,
    bool IsDebit // True ise Borç (Giriş), False ise Alacak (Çıkış)
) : IRequest<Result<string>>;
}
