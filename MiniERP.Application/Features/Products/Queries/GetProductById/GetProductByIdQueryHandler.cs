using AutoMapper;
using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Features.Products.Queries.GetProductById
{
    public sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<GetProductByIdResponse>>
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IMapper _mapper;

        public GetProductByIdQueryHandler(IRepository<Product> productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<Result<GetProductByIdResponse>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken,
                p => p.Category,
                p => p.Brand,
                p => p.Unit,
                p => p.Warehouse);

            if (product is null)
                return Result<GetProductByIdResponse>.Failure("Ürün bulunamadı.");

            var response = _mapper.Map<GetProductByIdResponse>(product);

            return Result<GetProductByIdResponse>.Success(response, "Ürün başarıyla getirildi");
        }
    }
}
