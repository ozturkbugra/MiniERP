using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.Transactions.Commands.AddOpeningBalance;
using MiniERP.Application.Features.Transactions.Commands.CancelTransaction;
using MiniERP.Application.Features.Transactions.Commands.CreateCollection;
using MiniERP.Application.Features.Transactions.Commands.MakePayment;
using MiniERP.Application.Features.Transactions.Commands.TransferMoney;
using MiniERP.Application.Features.Transactions.Queries.GetAccountBalance;
using MiniERP.Application.Features.Transactions.Queries.GetBankStatement;
using MiniERP.Application.Features.Transactions.Queries.GetCashStatement;
using MiniERP.Application.Features.Transactions.Queries.GetCustomerListWithBalance;
using MiniERP.Application.Features.Transactions.Queries.GetCustomerStatement;
using MiniERP.Application.Features.Transactions.Queries.GetFinancialStatus;
using MiniERP.Application.Features.Transactions.Queries.GetFinancialSummary;
using MiniERP.Domain.Enums;

namespace MiniERP.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator) => _mediator = mediator;

    [HttpPost("Collection")]
    [Authorize(Policy = AppPermissions.Transactions.Create)]
    public async Task<IActionResult> Collection(CreateCollectionCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("Collection/{transactionId}")]
    [Authorize(Policy = AppPermissions.Transactions.Cancel)]
    public async Task<IActionResult> DeleteCollection(Guid transactionId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CancelTransactionCommand(transactionId), cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("Payment")]
    [Authorize(Policy = AppPermissions.Transactions.Create)]
    public async Task<IActionResult> Payment(MakePaymentCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("Payment/{transactionId}")]
    [Authorize(Policy = AppPermissions.Transactions.Cancel)]
    public async Task<IActionResult> DeletePayment(Guid transactionId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CancelTransactionCommand(transactionId), cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("Transfer")]
    [Authorize(Policy = AppPermissions.Transactions.Create)]
    public async Task<IActionResult> Transfer(TransferMoneyCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("Transfer/{transactionId}")]
    [Authorize(Policy = AppPermissions.Transactions.Cancel)]
    public async Task<IActionResult> DeleteTransfer(Guid transactionId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CancelTransactionCommand(transactionId), cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("OpeningBalance")]
    [Authorize(Policy = AppPermissions.Transactions.Create)]
    public async Task<IActionResult> OpeningBalance(AddOpeningBalanceCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("OpeningBalance/{transactionId}")]
    [Authorize(Policy = AppPermissions.Transactions.Cancel)]
    public async Task<IActionResult> OpeningBalance(Guid transactionId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CancelTransactionCommand(transactionId), cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("CustomerStatement/{customerId}")]
    [Authorize(Policy = AppPermissions.Transactions.View)]
    public async Task<IActionResult> GetCustomerStatement(Guid customerId, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetCustomerStatementQuery(customerId), cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpGet("BankStatement/{bankId}")]
    [Authorize(Policy = AppPermissions.Transactions.View)]
    public async Task<IActionResult> GetBankStatement(Guid bankId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetBankStatementQuery(bankId), cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("CashStatement/{cashId}")]
    [Authorize(Policy = AppPermissions.Transactions.View)]
    public async Task<IActionResult> GetCashStatement(Guid cashId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCashStatementQuery(cashId), cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("CustomerBalances")]
    [Authorize(Policy = AppPermissions.Transactions.View)]
    public async Task<IActionResult> GetCustomerBalances(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCustomerListWithBalanceQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("FinancialSummary")]
    [Authorize(Policy = AppPermissions.Transactions.View)]
    public async Task<IActionResult> GetFinancialSummary(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetFinancialSummaryQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("AccountBalance/{accountId}")]
    [Authorize(Policy = AppPermissions.Transactions.View)]
    public async Task<IActionResult> GetAccountBalance(Guid accountId, [FromQuery] bool isBank, [FromQuery] DateTime? date, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetAccountBalanceQuery(accountId, isBank, date), cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpGet("FinancialStatus")]
    [Authorize(Policy = AppPermissions.Transactions.View)]
    public async Task<IActionResult> GetFinancialStatus(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetFinancialStatusQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}