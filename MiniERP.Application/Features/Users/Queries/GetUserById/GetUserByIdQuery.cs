using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(string Id) : IRequest<Result<GetUserByIdQueryResponse>>;