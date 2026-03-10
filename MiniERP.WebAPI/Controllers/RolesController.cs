using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.Roles.Commands.AssignPermissions;
using MiniERP.Application.Features.Roles.Commands.CreateRoles;
using MiniERP.Application.Features.Roles.Commands.DeleteRoles;
using MiniERP.Application.Features.Roles.Commands.UpdateRoles;
using MiniERP.Application.Features.Roles.Queries.GetAllPermissions;
using MiniERP.Application.Features.Roles.Queries.GetAllRoles;
using MiniERP.Application.Features.Roles.Queries.GetPermissionsByRoleId;
using MiniERP.Application.Features.Roles.Queries.GetRoleById;
using MiniERP.Domain.Enums;

namespace MiniERP.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class RolesController : ControllerBase
{
    private readonly IMediator _mediator;

    public RolesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = AppPermissions.Roles.View)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetAllRolesQuery(), cancellationToken);
        return Ok(response);
    }

    [HttpPost("CreateRole")]
    [Authorize(Policy = AppPermissions.Roles.Create)]
    public async Task<IActionResult> CreateRole(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = AppPermissions.Roles.View)]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetRoleByIdQuery(id), cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = AppPermissions.Roles.Update)]
    public async Task<IActionResult> Update(Guid id, UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        if (id.ToString() != request.Id)
        {
            return BadRequest("URL'deki ID ile gönderilen verideki ID uyuşmuyor.");
        }

        var response = await _mediator.Send(request, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AppPermissions.Roles.Delete)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new DeleteRoleCommand(id), cancellationToken);
        return Ok(response);
    }

    [HttpPost("AssignPermissions")]
    [Authorize(Policy = AppPermissions.Roles.Update)]
    public async Task<IActionResult> AssignPermissions([FromBody] AssignPermissionsToRoleCommand command,CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpGet("GetPermissionsByRoleId/{roleId}")]
    [Authorize(Policy = AppPermissions.Roles.View)]
    public async Task<IActionResult> GetPermissionsByRoleId(Guid roleId, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetPermissionsByRoleIdQuery(roleId), cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpGet("GetAllPermissions")]
    [Authorize(Policy = AppPermissions.Roles.View)]
    public async Task<IActionResult> GetAllPermissions(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetAllPermissionsQuery(), cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }
}