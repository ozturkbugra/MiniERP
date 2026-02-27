using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Transactions.Commands.TransferMoney
{
    public sealed record TransferMoneyCommand(
    DateTime Date,
    string Description,
    decimal Amount,
    // Kaynak (Nereden çıkıyor)
    Guid? FromCashId,
    Guid? FromBankId,
    // Hedef (Nereye giriyor)
    Guid? ToCashId,
    Guid? ToBankId
) : IRequest<Result<string>>;
}
