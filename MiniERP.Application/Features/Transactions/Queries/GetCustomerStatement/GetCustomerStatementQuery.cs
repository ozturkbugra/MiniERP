using MediatR;
using MiniERP.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.Transactions.Queries.GetCustomerStatement
{
    public sealed record GetCustomerStatementQuery(Guid CustomerId) : IRequest<Result<List<CustomerStatementDto>>>;
}
