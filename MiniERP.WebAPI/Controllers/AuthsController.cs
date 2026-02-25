using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.AssignRole.Commands.AssingRole;
using MiniERP.Application.Features.AssignRole.Commands.RemoveRoleFromUser;
using MiniERP.Application.Features.Auth.Commands.LoginCommands;
using MiniERP.Application.Features.Users.Commands.ChangePassword;
using MiniERP.Application.Features.Users.Commands.CreateUser;
using MiniERP.Application.Features.Users.Commands.DeleteUser;
using MiniERP.Application.Features.Users.Commands.UpdateUser;
using MiniERP.Application.Features.Users.Queries.GetAllUsers;
using MiniERP.Application.Features.Users.Queries.GetUserById;

namespace MiniERP.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);

            return Ok(response);
        }

        [HttpPost("AssignRole")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole(AssignRoleCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpDelete("remove-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveRoleFromUser(RemoveRoleFromUserCommand request)
        {
            var response = await _mediator.Send(request);
            return Ok(response);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);

            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpGet("GetAll")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetAllUsersQuery(), cancellationToken);
            return Ok(response);
        }

        [HttpGet("GetById/{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetUserByIdQuery(id), cancellationToken);
            return Ok(response);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(Guid id, UpdateUserCommand request, CancellationToken cancellationToken)
        {
            if (id.ToString() != request.Id)
            {
                return BadRequest("URL'deki ID ile gönderilen kullanıcı verisindeki ID uyuşmuyor.");
            }

            var response = await _mediator.Send(request, cancellationToken);

            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpDelete("{id}")] 
        [Authorize] 
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new DeleteUserCommand(id), cancellationToken);
            return Ok(response);
        }

        [HttpPost("change-password")]
        [Authorize] 
        public async Task<IActionResult> ChangePassword(ChangePasswordCommand request)
        {
            var response = await _mediator.Send(request);
            return Ok(response);
        }

    }
}
