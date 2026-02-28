using AutoMapper;
using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Features.Brands.Commands.CreateBrand
{
    public sealed class CreateBrandCommandHandler : IRequestHandler<CreateBrandCommand, Result<string>>
    {
        private readonly IRepository<Brand> _brandRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateBrandCommandHandler(IRepository<Brand> brandRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _brandRepository = brandRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<string>> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
        {
            var isNameExists = await _brandRepository.AnyAsync(x => x.Name.ToLower() == request.Name.ToLower(), cancellationToken);
            if (isNameExists)
                return Result<string>.Failure("Bu marka ismi zaten sistemde mevcut.");

            var brand = _mapper.Map<Brand>(request);

            await _brandRepository.AddAsync(brand, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(brand.Name, "Marka başarıyla oluşturuldu.");
        }
    }
}
