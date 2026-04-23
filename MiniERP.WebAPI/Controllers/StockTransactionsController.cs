using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.StockTransactions.Commands.CreateStockTransaction;
using MiniERP.Application.Features.StockTransactions.Commands.DeleteStockTransaction;
using MiniERP.Application.Features.StockTransactions.Queries.GetAllStockTransactions;
using MiniERP.Application.Features.StockTransactions.Queries.GetCriticalStock;
using MiniERP.Application.Features.StockTransactions.Queries.GetStockBalance;
using MiniERP.Application.Features.StockTransactions.Queries.GetStockTransactions;
using MiniERP.Domain.Enums;

namespace MiniERP.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class StockTransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StockTransactionsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = AppPermissions.StockTransactions.View)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetAllStockTransactionsQuery(), cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost]
    [Authorize(Policy = AppPermissions.StockTransactions.Create)]
    public async Task<IActionResult> Create([FromBody] CreateStockTransactionCommand command, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpGet("GetTransaction")]
    [Authorize(Policy = AppPermissions.StockTransactions.View)]
    public async Task<IActionResult> GetTransaction([FromQuery] GetStockTransactionsQuery query, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(query, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AppPermissions.StockTransactions.Delete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new DeleteStockTransactionCommand(id), cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpGet("balance")]
    [Authorize(Policy = AppPermissions.StockTransactions.View)]
    public async Task<IActionResult> GetBalance([FromQuery] GetStockBalanceQuery query)
    {
        var response = await _mediator.Send(query);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpGet("critical-stock")]
    [Authorize(Policy = AppPermissions.Reports.View)]
    public async Task<IActionResult> GetCriticalStock([FromQuery] GetCriticalStockQuery query, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(query, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }
}