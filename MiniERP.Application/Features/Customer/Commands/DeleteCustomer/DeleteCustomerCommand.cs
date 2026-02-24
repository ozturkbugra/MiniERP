using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Customer.Commands.DeleteCustomer
{
    public sealed record DeleteCustomerCommand(Guid Id) : IRequest<Result<string>>;
}

