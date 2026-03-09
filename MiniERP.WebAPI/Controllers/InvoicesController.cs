using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.Invoices.Commands.ApproveInvoice;
using MiniERP.Application.Features.Invoices.Commands.ApproveReturnInvoice;
using MiniERP.Application.Features.Invoices.Commands.CancelInvoice;
using MiniERP.Application.Features.Invoices.Commands.CreateInvoice;
using MiniERP.Application.Features.Invoices.Commands.CreateReturnInvoice;
using MiniERP.Application.Features.Invoices.Queries.GetAllInvoices;
using MiniERP.Application.Features.Invoices.Queries.GetInvoiceById;
using MiniERP.Domain.Enums;

namespace MiniERP.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class InvoicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public InvoicesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = AppPermissions.Invoices.View)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetInvoicesQuery(), cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = AppPermissions.Invoices.View)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetInvoiceByIdQuery(id), cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost]
    [Authorize(Policy = AppPermissions.Invoices.Create)]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost("Approve")]
    [Authorize(Policy = AppPermissions.Invoices.Approve)]
    public async Task<IActionResult> Approve([FromBody] ApproveInvoiceCommand request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost("Cancel")]
    [Authorize(Policy = AppPermissions.Invoices.Cancel)]
    public async Task<IActionResult> Cancel([FromBody] CancelInvoiceCommand request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost("return")]
    [Authorize(Policy = AppPermissions.Invoices.Return)]
    public async Task<IActionResult> CreateReturnInvoice(CreateReturnInvoiceCommand command, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost("return/approve")]
    [Authorize(Policy = AppPermissions.Invoices.Approve)]
    public async Task<IActionResult> ApproveReturnInvoice(ApproveReturnInvoiceCommand command, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }
}