using MediatR;
using MiniERP.Domain.Common;
using MiniERP.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.Customer.Commands.UpdateCustomer
{
    public sealed record UpdateCustomerCommand(
    Guid Id,
    string Name,
    string? TaxDepartment,
    string? TaxNumber,
    string? Phone,
    string? Email,
    string? Address,
    CustomerType Type) : IRequest<Result<string>>;
}
