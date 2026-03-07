using MediatR;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.Orders.Commands.ApproveOrder;
using MiniERP.Application.Features.Orders.Commands.CancelOrder;
using MiniERP.Application.Features.Orders.Commands.CreateOrder;
using MiniERP.Application.Features.Orders.Queries;

namespace MiniERP.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetOrdersQuery(), cancellationToken);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderCommand command, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(command, cancellationToken);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new ApproveOrderCommand(id), cancellationToken);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new CancelOrderCommand(id), cancellationToken);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
    }
}
