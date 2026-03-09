using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.Products.Commands.CreateProduct;
using MiniERP.Application.Features.Products.Commands.DeleteProduct;
using MiniERP.Application.Features.Products.Commands.UpdateProduct;
using MiniERP.Application.Features.Products.Queries.GetAllProducts;
using MiniERP.Application.Features.Products.Queries.GetProductById;
using MiniERP.Domain.Enums;

namespace MiniERP.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = AppPermissions.Products.View)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetAllProductsQuery(), cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = AppPermissions.Products.View)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetProductByIdQuery(id), cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost]
    [Authorize(Policy = AppPermissions.Products.Create)]
    public async Task<IActionResult> Create(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = AppPermissions.Products.Update)]
    public async Task<IActionResult> Update(Guid id, UpdateProductCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("ID uyuşmazlığı tespit edildi.");

        var response = await _mediator.Send(command, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AppPermissions.Products.Delete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new DeleteProductCommand(id), cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }
}