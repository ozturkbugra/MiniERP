using MediatR;
using MiniERP.Domain.Common;
using MiniERP.Domain.Enums;

namespace MiniERP.Application.Features.Invoices.Commands.ApproveInvoice
{
    public sealed record ApproveInvoiceCommand(
        Guid Id,
        PaymentType PaymentType = PaymentType.Credit,
        Guid? CashId = null,
        Guid? BankId = null
    ) : IRequest<Result<string>>;
}
