using MediatR;
using MiniERP.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.Transactions.Commands.MakePayment
{
    public sealed record MakePaymentCommand(
    DateTime Date,
    string Description,
    decimal Amount,
    Guid CustomerId,
    Guid? CashId,
    Guid? BankId
) : IRequest<Result<string>>;
}
