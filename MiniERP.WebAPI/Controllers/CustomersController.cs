using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.Customer.Commands.CreateCustomer;
using MiniERP.Application.Features.Customer.Commands.DeleteCustomer;
using MiniERP.Application.Features.Customer.Commands.UpdateCustomer;
using MiniERP.Application.Features.Customer.Queries.GetAllCustomers;
using MiniERP.Application.Features.Customer.Queries.GetCustomerById;
using MiniERP.Domain.Enums;

namespace MiniERP.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = AppPermissions.Customers.View)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllCustomersQuery());
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = AppPermissions.Customers.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetCustomerByIdQuery(id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost]
    [Authorize(Policy = AppPermissions.Customers.Create)]
    public async Task<IActionResult> Create(CreateCustomerCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = AppPermissions.Customers.Update)]
    public async Task<IActionResult> Update(Guid id, UpdateCustomerCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(new { Message = "URL'deki ID ile gönderilen verideki ID uyuşmuyor." });
        }

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AppPermissions.Customers.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteCustomerCommand(id));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}