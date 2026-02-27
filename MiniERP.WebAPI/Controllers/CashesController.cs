using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.Cash.Commands.CreateCash;
using MiniERP.Application.Features.Cashs.Commands.DeleteCash;
using MiniERP.Application.Features.Cashs.Commands.UpdateCash;
using MiniERP.Application.Features.Cashs.Queries.GetAllCashes;
using MiniERP.Application.Features.Cashs.Queries.GetCashById;

namespace MiniERP.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CashesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CashesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _mediator.Send(new GetAllCashesQuery());
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var response = await _mediator.Send(new GetCashByIdQuery(id));
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCashCommand command)
        {
            var response = await _mediator.Send(command);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateCashCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("URL'deki ID ile gövdedeki ID uyuşmuyor.");
            }

            var response = await _mediator.Send(command);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _mediator.Send(new DeleteCashCommand(id));
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
    }
}
