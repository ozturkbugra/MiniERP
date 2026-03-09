using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.Units.Commands.CreateUnit;
using MiniERP.Application.Features.Units.Commands.DeleteUnit;
using MiniERP.Application.Features.Units.Commands.UpdateUnit;
using MiniERP.Application.Features.Units.Queries.GetAllUnits;
using MiniERP.Application.Features.Units.Queries.GetUnitById;
using MiniERP.Domain.Enums;

namespace MiniERP.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class UnitsController : ControllerBase
{
    private readonly IMediator _mediator;

    public UnitsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = AppPermissions.Units.View)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetAllUnitsQuery(), cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = AppPermissions.Units.View)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetUnitByIdQuery(id), cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost]
    [Authorize(Policy = AppPermissions.Units.Create)]
    public async Task<IActionResult> Create(CreateUnitCommand command, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = AppPermissions.Units.Update)]
    public async Task<IActionResult> Update(Guid id, UpdateUnitCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("URL'deki ID ile gönderilen verideki ID uyuşmuyor.");

        var response = await _mediator.Send(command, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AppPermissions.Units.Delete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new DeleteUnitCommand(id), cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }
}