using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.StockTransactions.Commands.DeleteStockTransaction
{
    public sealed record DeleteStockTransactionCommand(Guid Id) : IRequest<Result<string>>;
}
