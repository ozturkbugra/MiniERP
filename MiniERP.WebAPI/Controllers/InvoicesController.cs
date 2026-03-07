using MediatR;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.Invoices.Commands.ApproveInvoice;
using MiniERP.Application.Features.Invoices.Commands.CancelInvoice;
using MiniERP.Application.Features.Invoices.Commands.CreateInvoice;
using MiniERP.Application.Features.Invoices.Queries;

namespace MiniERP.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InvoicesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetInvoicesQuery(), cancellationToken);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInvoiceCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPost("Approve")]
        public async Task<IActionResult> Approve([FromBody] ApproveInvoiceCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPost("Cancel")]
        public async Task<IActionResult> Cancel([FromBody] CancelInvoiceCommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
    }
}
