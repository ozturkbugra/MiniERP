using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.AssignRole.Commands.AssingRole;

// React veya Postman'den bize Kullanıcı ID'si ve eklenecek Rolün adı gelecek.
public sealed record AssignRoleCommand(
    string UserId,
    string RoleName) : IRequest<Result>;