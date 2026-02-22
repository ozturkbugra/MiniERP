using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Users.Queries.GetAllUsers
{
    public sealed record GetAllUsersQuery() : IRequest<Result<List<GetAllUsersQueryResponse>>>;
}
