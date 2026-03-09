using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.Warehouses.Commands.CreateWarehouse;
using MiniERP.Application.Features.Warehouses.Commands.DeleteWarehouse;
using MiniERP.Application.Features.Warehouses.Commands.UpdateWarehouse;
using MiniERP.Application.Features.Warehouses.Queries.GetAllWarehouses;
using MiniERP.Application.Features.Warehouses.Queries.GetWarehouseById;
using MiniERP.Domain.Enums;

namespace MiniERP.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class WarehousesController : ControllerBase
{
    private readonly IMediator _mediator;

    public WarehousesController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [Authorize(Policy = AppPermissions.Warehouses.Create)]
    public async Task<IActionResult> Create(CreateWarehouseCommand command, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpGet]
    [Authorize(Policy = AppPermissions.Warehouses.View)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetAllWarehousesQuery(), cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = AppPermissions.Warehouses.View)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetWarehouseByIdQuery(id), cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = AppPermissions.Warehouses.Update)]
    public async Task<IActionResult> Update(Guid id, UpdateWarehouseCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("ID uyuşmazlığı tespit edildi.");

        var response = await _mediator.Send(command, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AppPermissions.Warehouses.Delete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new DeleteWarehouseCommand(id), cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }
}