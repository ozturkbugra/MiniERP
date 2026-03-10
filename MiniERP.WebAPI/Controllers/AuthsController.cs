using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.AssignRole.Commands.AssingRole;
using MiniERP.Application.Features.AssignRole.Commands.RemoveRoleFromUser;
using MiniERP.Application.Features.Auth.Commands.LoginCommands;
using MiniERP.Application.Features.Auth.Commands.Logout;
using MiniERP.Application.Features.Users.Commands.ChangePassword;
using MiniERP.Application.Features.Users.Commands.CreateUser;
using MiniERP.Application.Features.Users.Commands.DeleteUser;
using MiniERP.Application.Features.Users.Commands.UpdateUser;
using MiniERP.Application.Features.Users.Queries.GetAllUsers;
using MiniERP.Application.Features.Users.Queries.GetMyPermissions;
using MiniERP.Application.Features.Users.Queries.GetUserById;
using MiniERP.Domain.Enums;
using System.Security.Claims;

namespace MiniERP.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class AuthsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // --- KULLANICI İŞLEMLERİ ---

        [HttpPost("Register")]
        [Authorize(Policy = AppPermissions.Users.Create)]
        public async Task<IActionResult> Register(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("GetAll")]
        [Authorize(Policy = AppPermissions.Users.View)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetAllUsersQuery(), cancellationToken);
            return Ok(response);
        }

        [HttpGet("GetById/{id}")]
        [Authorize(Policy = AppPermissions.Users.View)]
        public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetUserByIdQuery(id), cancellationToken);
            return Ok(response);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = AppPermissions.Users.Update)]
        public async Task<IActionResult> Update(Guid id, UpdateUserCommand request, CancellationToken cancellationToken)
        {
            if (id.ToString() != request.Id)
            {
                return BadRequest("ID uyuşmazlığı var.");
            }
            var response = await _mediator.Send(request, cancellationToken);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = AppPermissions.Users.Delete)]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new DeleteUserCommand(id), cancellationToken);
            return Ok(response);
        }

        // --- ROL ATAMA İŞLEMLERİ ---

        [HttpPost("AssignRole")]
        [Authorize(Policy = AppPermissions.Roles.Update)]
        public async Task<IActionResult> AssignRole(AssignRoleCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpDelete("remove-role")]
        [Authorize(Policy = AppPermissions.Roles.Update)]
        public async Task<IActionResult> RemoveRoleFromUser(RemoveRoleFromUserCommand request)
        {
            var response = await _mediator.Send(request);
            return Ok(response);
        }

        // --- KİMLİK DOĞRULAMA (HERKESE AÇIK VEYA STANDART ÜYE) ---

        [HttpPost("Login")]
        [AllowAnonymous] // Giriş yapmak için giriş yapmış olmak gerekmez
        public async Task<IActionResult> Login(LoginCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPost("change-password")]
        [Authorize] // Sadece giriş yapmış olması yeterli, her kullanıcı kendi şifresini değiştirebilir.
        public async Task<IActionResult> ChangePassword(ChangePasswordCommand request)
        {
            var response = await _mediator.Send(request);
            return Ok(response);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var response = await _mediator.Send(new LogoutCommand());
            return Ok(new { message = response });
        }

        [HttpGet("GetMyPermissions")]
        [Authorize]
        public async Task<IActionResult> GetMyPermissions(CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var parsedUserId))
            {
                return Unauthorized(new { Message = "Kullanıcı kimliği doğrulanamadı. Lütfen tekrar giriş yapın." });
            }
            var response = await _mediator.Send(new GetMyPermissionsQuery(parsedUserId), cancellationToken);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
    }
}