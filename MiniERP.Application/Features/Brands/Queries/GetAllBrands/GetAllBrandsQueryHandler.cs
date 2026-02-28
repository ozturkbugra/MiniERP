using AutoMapper;
using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Features.Brands.Queries.GetAllBrands
{
    public sealed class GetAllBrandsQueryHandler : IRequestHandler<GetAllBrandsQuery, Result<List<GetAllBrandsResponse>>>
    {
        private readonly IRepository<Brand> _brandRepository;
        private readonly IMapper _mapper;

        public GetAllBrandsQueryHandler(IRepository<Brand> brandRepository, IMapper mapper)
        {
            _brandRepository = brandRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<GetAllBrandsResponse>>> Handle(GetAllBrandsQuery request, CancellationToken cancellationToken)
        {
            var brands = await _brandRepository.GetAllAsync(cancellationToken);
            var response = _mapper.Map<List<GetAllBrandsResponse>>(brands);

            return Result<List<GetAllBrandsResponse>>.Success(response, "Markalar başarıyla getirildi.");
        }
    }
}
