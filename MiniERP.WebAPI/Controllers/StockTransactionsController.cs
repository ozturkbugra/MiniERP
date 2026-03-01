using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.StockTransactions.Commands;
using MiniERP.Application.Features.StockTransactions.Queries.GetAllStockTransactions;
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
        public async Task<IActionResult> Create(CreateStockTransactionCommand command, CancellationToken cancellationToken)
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


    }
}
