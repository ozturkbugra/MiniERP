using MediatR;
using MiniERP.Domain.Common;
using MiniERP.Domain.Enums;

namespace MiniERP.Application.Features.StockTransactions.Commands.CreateStockTransaction
{
    public sealed record CreateStockTransactionCommand(
    string DocumentNo,
    DateTime TransactionDate,
    decimal Quantity,
    decimal UnitPrice,
    string? Description,
    StockTransactionType Type, // In (Giriş) - Out (Çıkış)
    Guid ProductId,
    Guid WarehouseId,
    Guid CustomerId,
    PaymentType PaymentType,
    Guid? CashId,
    Guid? BankId
) : IRequest<Result<string>>;
}
