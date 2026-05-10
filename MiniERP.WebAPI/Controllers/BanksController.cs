using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.Banks.Commands.CreateBank;
using MiniERP.Application.Features.Banks.Commands.DeleteBank;
using MiniERP.Application.Features.Banks.Commands.UpdateBank;
using MiniERP.Application.Features.Banks.Queries.GetAllBanks;
using MiniERP.Application.Features.Banks.Queries.GetBankById;
using MiniERP.Domain.Enums;

namespace MiniERP.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Temel giriş kontrolü
    public sealed class BanksController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BanksController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(Policy = AppPermissions.Banks.View)]
        public async Task<IActionResult> GetAll()
        {
            var response = await _mediator.Send(new GetAllBanksQuery());
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = AppPermissions.Banks.View)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var response = await _mediator.Send(new GetBankByIdQuery(id));
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPost]
        [Authorize(Policy = AppPermissions.Banks.Create)]
        public async Task<IActionResult> Create(CreateBankCommand command)
        {
            var response = await _mediator.Send(command);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = AppPermissions.Banks.Update)]
        public async Task<IActionResult> Update(Guid id, UpdateBankCommand command)
        {
            if (id.ToString().ToUpper() != command.Id.ToString().ToUpper())
            {
                return BadRequest("URL'deki ID ile gönderilen verideki ID uyuşmuyor.");
            }

            var response = await _mediator.Send(command);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = AppPermissions.Banks.Delete)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _mediator.Send(new DeleteBankCommand(id));
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
    }
}