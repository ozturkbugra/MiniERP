using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.Cash.Commands.CreateCash;
using MiniERP.Application.Features.Cashs.Commands.DeleteCash;
using MiniERP.Application.Features.Cashs.Commands.UpdateCash;
using MiniERP.Application.Features.Cashs.Queries.GetAllCashes;
using MiniERP.Application.Features.Cashs.Queries.GetCashById;
using MiniERP.Domain.Enums;

namespace MiniERP.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class CashesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CashesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = AppPermissions.Cashes.View)]
    public async Task<IActionResult> GetAll()
    {
        var response = await _mediator.Send(new GetAllCashesQuery());
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = AppPermissions.Cashes.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _mediator.Send(new GetCashByIdQuery(id));
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost]
    [Authorize(Policy = AppPermissions.Cashes.Create)]
    public async Task<IActionResult> Create(CreateCashCommand command)
    {
        var response = await _mediator.Send(command);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = AppPermissions.Cashes.Update)]
    public async Task<IActionResult> Update(Guid id, UpdateCashCommand command)
    {
        if (id != command.Id) return BadRequest("ID uyuşmazlığı.");
        var response = await _mediator.Send(command);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AppPermissions.Cashes.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var response = await _mediator.Send(new DeleteCashCommand(id));
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }
}