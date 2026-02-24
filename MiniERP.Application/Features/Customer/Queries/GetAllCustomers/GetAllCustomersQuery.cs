using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Customer.Queries.GetAllCustomers
{
    public sealed record GetAllCustomersQuery() : IRequest<Result<List<GetAllCustomersQueryResponse>>>;
}
