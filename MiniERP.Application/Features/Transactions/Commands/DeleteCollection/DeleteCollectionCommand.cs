using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Transactions.Commands.DeleteCollection
{
    public sealed record DeleteCollectionCommand(Guid TransactionId) : IRequest<Result<string>>;
}
