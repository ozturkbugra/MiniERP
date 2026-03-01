using MediatR;
using MiniERP.Domain.Common;
using MiniERP.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.StockTransactions.Commands
{
    public sealed record CreateStockTransactionCommand(
    string DocumentNo,
    DateTime TransactionDate,
    decimal Quantity,
    decimal UnitPrice,
    string? Description,
    StockTransactionType Type, // 1: Giriş, 2: Çıkış
    Guid ProductId,
    Guid WarehouseId,
    Guid CustomerId) : IRequest<Result<string>>;
}
