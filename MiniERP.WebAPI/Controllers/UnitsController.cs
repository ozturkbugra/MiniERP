using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.Units.Commands.CreateUnit;
using MiniERP.Application.Features.Units.Commands.DeleteUnit;
using MiniERP.Application.Features.Units.Commands.UpdateUnit;
using MiniERP.Application.Features.Units.Queries.GetAllUnits;
using MiniERP.Application.Features.Units.Queries.GetUnitById;

namespace MiniERP.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UnitsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UnitsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetAllUnitsQuery(), cancellationToken);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUnitCommand command, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(command, cancellationToken);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetUnitByIdQuery(id), cancellationToken);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateUnitCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id)
                return BadRequest("URL'deki ID ile gönderilen verideki ID uyuşmuyor.");

            var response = await _mediator.Send(command, cancellationToken);

            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new DeleteUnitCommand(id), cancellationToken);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
    }
}
