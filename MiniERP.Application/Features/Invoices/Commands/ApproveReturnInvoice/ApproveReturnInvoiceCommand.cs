using MediatR;
using MiniERP.Domain.Common;
using MiniERP.Domain.Enums;

namespace MiniERP.Application.Features.Invoices.Commands.ApproveReturnInvoice
{
    public sealed record ApproveReturnInvoiceCommand(
        Guid Id,
        PaymentType PaymentType,
        Guid? CashId = null,
        Guid? BankId = null) : IRequest<Result<string>>;
}
