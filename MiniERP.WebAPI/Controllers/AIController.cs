using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.AI.Queries.GetAIChatResponse;

namespace MiniERP.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AIController(IMediator mediator) => _mediator = mediator;

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] GetAIChatResponseQuery query)
        {
            var result = await _mediator.Send(query);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
