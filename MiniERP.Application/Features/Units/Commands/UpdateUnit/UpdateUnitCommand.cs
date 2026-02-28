using MediatR;
using MiniERP.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.Units.Commands.UpdateUnit
{
    public sealed record UpdateUnitCommand(
    Guid Id,
    string Code,
    string Name) : IRequest<Result<string>>;
}
