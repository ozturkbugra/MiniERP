using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.Customer.Commands.CreateCustomer;
using MiniERP.Application.Features.Customer.Commands.DeleteCustomer;

namespace MiniERP.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCustomerCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _mediator.Send(new DeleteCustomerCommand(id));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
