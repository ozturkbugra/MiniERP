using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Users.Queries.GetMyPermissions
{
    public sealed record GetMyPermissionsQuery(Guid UserId) : IRequest<Result<List<string>>>;
}
