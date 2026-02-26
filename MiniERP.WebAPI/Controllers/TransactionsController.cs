using MediatR;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.Transactions.Commands.CreateCollection;

namespace MiniERP.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TransactionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Tahsilat (Müşteriden Para Alma)
        [HttpPost("Collection")]
        public async Task<IActionResult> Collection(CreateCollectionCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
