using MediatR;
using MiniERP.Domain.Common;
using MiniERP.Domain.Enums;

namespace MiniERP.Application.Features.Customer.Commands.CreateCustomer
{
    public sealed record CreateCustomerCommand(
     string Name,
     string? TaxDepartment,
     string? TaxNumber,
     string? Phone,
     string? Email,
     string? Address,
     CustomerType Type) : IRequest<Result<string>>;
}
