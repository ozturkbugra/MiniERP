using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.Transactions.Queries.GetAccountBalance
{
    public sealed record AccountBalanceResponse(
    Guid AccountId,
    decimal Balance,
    DateTime CalculatedDate
);
}
