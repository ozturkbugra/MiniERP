using MediatR;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetAllRolesQuery(), cancellationToken);
            return Ok(response);
        }

        [HttpPost("CreateRole")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateRole(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetRoleByIdQuery(id), cancellationToken);
            return Ok(response);
        }

        [HttpPut("{id}")] 
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, UpdateRoleCommand request, CancellationToken cancellationToken)
        {
            if (id.ToString() != request.Id)
            {
                return BadRequest("URL'deki ID ile gönderilen verideki ID uyuşmuyor.");
            }

            var response = await _mediator.Send(request, cancellationToken);

            // Result<T> yapımıza uygun olarak sonucu dönüyoruz
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new DeleteRoleCommand(id), cancellationToken);
            return Ok(response);
        }
    }
}
