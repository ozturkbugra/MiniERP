using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.Reports.Queries.GetDashboardSummary;

namespace MiniERP.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("dashboard-summary")]
        public async Task<IActionResult> GetDashboardSummary(CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetDashboardSummaryQuery(), cancellationToken);
            return response.IsSuccess ? Ok(response) : BadRequest(response);

        }
    }
}
