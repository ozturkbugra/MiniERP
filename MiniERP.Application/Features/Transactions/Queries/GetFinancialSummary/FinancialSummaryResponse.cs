using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.Transactions.Queries.GetFinancialSummary
{
    public sealed record FinancialSummaryResponse(
    decimal TotalCashBalance,
    decimal TotalBankBalance,
    decimal GeneralTotal // Cash + Bank
);
}
