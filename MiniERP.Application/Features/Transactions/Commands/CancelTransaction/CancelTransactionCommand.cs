using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Transactions.Commands.CancelTransaction
{
    public sealed record CancelTransactionCommand(Guid TransactionId) : IRequest<Result<string>>;
}
