using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Transactions.Commands.CreateCollection
{
    public sealed record CreateCollectionCommand(
    DateTime Date,
    string Description,
    decimal Amount,
    Guid CustomerId,
    Guid? CashId, // Ya kasaya girecek
    Guid? BankId  // Ya da bankaya (ikisi birden olamaz)
) : IRequest<Result<string>>;
}
