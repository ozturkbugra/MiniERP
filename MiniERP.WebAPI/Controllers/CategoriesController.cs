using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.Categories.Command.CreateCategory;
using MiniERP.Application.Features.Categories.Command.DeleteCategory;
using MiniERP.Application.Features.Categories.Command.UpdateCategory;
using MiniERP.Application.Features.Categories.Queries.GetAllCategories;
using MiniERP.Application.Features.Categories.Queries.GetCategoryById;
using MiniERP.Domain.Enums;

namespace MiniERP.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = AppPermissions.Categories.View)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetAllCategoriesQuery(), cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = AppPermissions.Categories.View)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetCategoryByIdQuery(id), cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost]
    [Authorize(Policy = AppPermissions.Categories.Create)]
    public async Task<IActionResult> Create(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = AppPermissions.Categories.Update)]
    public async Task<IActionResult> Update(Guid id, UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest("ID uyuşmazlığı.");

        var response = await _mediator.Send(command, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AppPermissions.Categories.Delete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new DeleteCategoryCommand(id), cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }
}