using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.Customer.Commands.CreateCustomer;
using MiniERP.Application.Features.Customer.Commands.DeleteCustomer;
using MiniERP.Application.Features.Customer.Commands.UpdateCustomer;
using MiniERP.Application.Features.Customer.Queries.GetAllCustomers;
using MiniERP.Application.Features.Customer.Queries.GetCustomerById;

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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllCustomersQuery());
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetCustomerByIdQuery(id));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
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

        [HttpPut("{id}")] 
        public async Task<IActionResult> Update(Guid id, UpdateCustomerCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest(new { Message = "URL'deki ID ile gönderilen verideki ID uyuşmuyor." });
            }

            var result = await _mediator.Send(command);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
