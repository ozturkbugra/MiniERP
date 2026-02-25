using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Banks.Commands.DeleteBank
{
    public sealed record DeleteBankCommand(Guid Id) : IRequest<Result<string>>;
}
