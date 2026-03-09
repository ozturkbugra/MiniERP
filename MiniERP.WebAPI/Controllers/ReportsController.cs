using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.Reports.Queries.GetDashboardSummary;
using MiniERP.Domain.Enums;

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

        [Authorize(Policy = AppPermissions.Reports.View)]
        [HttpGet("dashboard-summary")]
        public async Task<IActionResult> GetDashboardSummary(CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetDashboardSummaryQuery(), cancellationToken);
            return response.IsSuccess ? Ok(response) : BadRequest(response);

        }
    }
}
