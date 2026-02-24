using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Customer.Queries.GetCustomerById
{
    public sealed record GetCustomerByIdQuery(Guid Id) : IRequest<Result<GetCustomerByIdQueryResponse>>;
}
