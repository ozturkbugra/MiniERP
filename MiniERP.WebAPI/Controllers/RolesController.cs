using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.Roles.Commands.CreateRoles;
using MiniERP.Application.Features.Roles.Queries.GetAllRoles;

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
        [Authorize] // Sadece yetkililer rolleri görebilmeli
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

        
    }
}
