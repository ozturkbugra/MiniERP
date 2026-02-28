using MediatR;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Application.Features.Brands.Commands.CreateBrand;
using MiniERP.Application.Features.Brands.Commands.DeleteBrand;
using MiniERP.Application.Features.Brands.Commands.UpdateBrand;
using MiniERP.Application.Features.Brands.Queries.GetAllBrands;
using MiniERP.Application.Features.Brands.Queries.GetBrandById;

namespace MiniERP.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BrandsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetAllBrandsQuery(), cancellationToken);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetBrandByIdQuery(id), cancellationToken);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateBrandCommand command, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(command, cancellationToken);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateBrandCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id)
                return BadRequest("URL'deki ID ile verideki ID uyuşmuyor.");

            var response = await _mediator.Send(command, cancellationToken);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new DeleteBrandCommand(id), cancellationToken);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
    }
}
