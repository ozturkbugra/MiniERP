using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.StockTransactions.Commands.CreateStockTransaction;
using MiniERP.Application.Features.StockTransactions.Commands.DeleteStockTransaction;
using MiniERP.Application.Features.StockTransactions.Queries.GetAllStockTransactions;
using MiniERP.Application.Features.StockTransactions.Queries.GetCriticalStock;
using MiniERP.Application.Features.StockTransactions.Queries.GetStockBalance;
using MiniERP.Application.Features.StockTransactions.Queries.GetStockTransactions;

namespace MiniERP.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockTransactionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StockTransactionsController(IMediator mediator) => _mediator = mediator;


        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetAllStockTransactionsQuery(), cancellationToken);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStockTransactionCommand command, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(command, cancellationToken);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpGet("GetTransaction")]
        public async Task<IActionResult> GetTransaction([FromQuery] GetStockTransactionsQuery query, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(query, cancellationToken);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new DeleteStockTransactionCommand(id), cancellationToken);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance([FromQuery] GetStockBalanceQuery query)
        {
            var response = await _mediator.Send(query);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpGet("critical-stock")]
        public async Task<IActionResult> GetCriticalStock([FromQuery] GetCriticalStockQuery query, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(query, cancellationToken);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
    }
}
