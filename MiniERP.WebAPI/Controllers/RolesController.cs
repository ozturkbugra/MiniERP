using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.Roles.Commands.CreateRoles;
using MiniERP.Application.Features.Roles.Commands.DeleteRoles;
using MiniERP.Application.Features.Roles.Commands.UpdateRoles;
using MiniERP.Application.Features.Roles.Queries.GetAllRoles;
using MiniERP.Application.Features.Roles.Queries.GetRoleById;

namespace MiniERP.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RolesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetAllRolesQuery(), cancellationToken);
            return Ok(response);
        }

        [HttpPost("CreateRole")]
        [Authorize]
        public async Task<IActionResult> CreateRole(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetRoleByIdQuery(id), cancellationToken);
            return Ok(response);
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Update(UpdateRoleCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new DeleteRoleCommand(id), cancellationToken);
            return Ok(response);
        }
    }
}
