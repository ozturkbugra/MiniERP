using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Invoices.Commands.CreateReturnInvoice
{
    public sealed record CreateReturnInvoiceCommand(
          Guid ParentInvoiceId,
          DateTime ReturnDate,
          List<ReturnInvoiceDetailRequest> Details
      ) : IRequest<Result<string>>;
}
