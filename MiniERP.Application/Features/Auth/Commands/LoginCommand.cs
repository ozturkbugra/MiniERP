using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Auth.Commands;

// Giriş başarılı olursa dışarıya string (Token) döneceğiz, o yüzden Result<string> dedik.
public sealed record LoginCommand(
    string Email,
    string Password) : IRequest<Result<string>>;