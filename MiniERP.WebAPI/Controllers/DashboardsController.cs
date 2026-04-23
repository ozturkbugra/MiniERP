using MediatR;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.Dashboard.Queries.GetDashboardSummary;
using MiniERP.Application.Features.Dashboard.Queries.GetStockSnapshot;
using MiniERP.Application.Features.Products.Queries.GetProductLedger;

namespace MiniERP.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DashboardsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("Summary")]
        public async Task<IActionResult> GetSummary()
        {
            // Özet verileri (Ciro, Borçlular, Stok vs.) getirir
            var response = await _mediator.Send(new GetDashboardSummaryQuery());

            if (response.IsSuccess)
                return Ok(response);

            return BadRequest(response);
        }

        // 🚀 YENİ: Ürün Defteri Endpoint'i
        [HttpGet("ProductLedger/{productId}")]
        public async Task<IActionResult> GetProductLedger(Guid productId)
        {
            // Belirli bir ürünün tüm alım/satım geçmişini getirir
            var response = await _mediator.Send(new GetProductLedgerQuery(productId));

            if (response.IsSuccess)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpGet("StockSnapshot")]
        public async Task<IActionResult> GetStockSnapshot([FromQuery] DateTime targetDate, [FromQuery] Guid? warehouseId)
        {
            var response = await _mediator.Send(new GetStockSnapshotQuery(targetDate, warehouseId));
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
    }
}