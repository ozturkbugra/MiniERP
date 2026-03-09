using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.Orders.Commands.ApproveOrder;
using MiniERP.Application.Features.Orders.Commands.CancelOrder;
using MiniERP.Application.Features.Orders.Commands.CreateOrder;
using MiniERP.Application.Features.Orders.Queries.GetAllOrders;
using MiniERP.Application.Features.Orders.Queries.GetOrderById;
using MiniERP.Domain.Enums;

namespace MiniERP.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = AppPermissions.Orders.View)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetOrdersQuery(), cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = AppPermissions.Orders.View)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetOrderByIdQuery(id), cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost]
    [Authorize(Policy = AppPermissions.Orders.Create)]
    public async Task<IActionResult> Create(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost("{id}/approve")]
    [Authorize(Policy = AppPermissions.Orders.Approve)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new ApproveOrderCommand(id), cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost("{id}/cancel")]
    [Authorize(Policy = AppPermissions.Orders.Cancel)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new CancelOrderCommand(id), cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }
}