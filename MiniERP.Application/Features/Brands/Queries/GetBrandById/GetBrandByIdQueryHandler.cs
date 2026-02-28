using AutoMapper;
using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.Brands.Queries.GetBrandById
{
    public sealed class GetBrandByIdQueryHandler : IRequestHandler<GetBrandByIdQuery, Result<GetBrandByIdResponse>>
    {
        private readonly IRepository<Brand> _brandRepository;
        private readonly IMapper _mapper;

        public GetBrandByIdQueryHandler(IRepository<Brand> brandRepository, IMapper mapper)
        {
            _brandRepository = brandRepository;
            _mapper = mapper;
        }

        public async Task<Result<GetBrandByIdResponse>> Handle(GetBrandByIdQuery request, CancellationToken cancellationToken)
        {
            var brand = await _brandRepository.GetByIdAsync(request.Id, cancellationToken);

            if (brand is null)
                return Result<GetBrandByIdResponse>.Failure("Marka bulunamadı.");

            var response = _mapper.Map<GetBrandByIdResponse>(brand);

            return Result<GetBrandByIdResponse>.Success(response,"Marka başarıyla getirildi.");
        }
    }
}
