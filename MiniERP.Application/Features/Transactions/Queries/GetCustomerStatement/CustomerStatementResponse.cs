using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.Transactions.Queries.GetCustomerStatement
{
    public sealed record CustomerStatementResponse(
    Guid TransactionId,
    DateTime Date,
    string Description,
    decimal Debit,
    decimal Credit,
    decimal Balance
);
}
