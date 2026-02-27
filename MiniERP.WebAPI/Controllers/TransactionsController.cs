using MediatR;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.Transactions.Commands.AddOpeningBalance;
using MiniERP.Application.Features.Transactions.Commands.CancelTransaction;
using MiniERP.Application.Features.Transactions.Commands.CreateCollection;
using MiniERP.Application.Features.Transactions.Commands.MakePayment;
using MiniERP.Application.Features.Transactions.Commands.TransferMoney;
using MiniERP.Application.Features.Transactions.Queries.GetCustomerStatement;

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

        [HttpDelete("Collection/{transactionId}")]
        public async Task<IActionResult> DeleteCollection(Guid transactionId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new CancelTransactionCommand(transactionId), cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // Ödeme (Satıcıya Ödeme)

        [HttpPost("Payment")]
        public async Task<IActionResult> Payment(MakePaymentCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("Payment/{transactionId}")]
        public async Task<IActionResult> DeletePayment(Guid transactionId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new CancelTransactionCommand(transactionId), cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // Virman
        [HttpPost("Transfer")]
        public async Task<IActionResult> Transfer(TransferMoneyCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("Transfer/{transactionId}")]
        public async Task<IActionResult> DeleteTransfer(Guid transactionId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new CancelTransactionCommand(transactionId), cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("OpeningBalance")]
        public async Task<IActionResult> OpeningBalance(AddOpeningBalanceCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("OpeningBalance/{transactionId}")]
        public async Task<IActionResult> OpeningBalance(Guid transactionId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new CancelTransactionCommand(transactionId), cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("CustomerStatement/{customerId}")]
        public async Task<IActionResult> GetCustomerStatement(Guid customerId, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetCustomerStatementQuery(customerId), cancellationToken);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
    }
}
