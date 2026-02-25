using MediatR;
using MiniERP.Domain.Common;
using MiniERP.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.Cash.Commands.CreateCash
{
    public sealed record CreateCashCommand(string Name, CurrencyType CurrencyType) : IRequest<Result<string>>;
}
