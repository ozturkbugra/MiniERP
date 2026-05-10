using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.AI.Queries.GetAIChatResponse
{
    public record GetAIChatResponseQuery(List<ChatMessageDto> History) : IRequest<Result<string>>;
}
