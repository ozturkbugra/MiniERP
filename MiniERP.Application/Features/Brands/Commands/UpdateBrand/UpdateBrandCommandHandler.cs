using AutoMapper;
using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Features.Brands.Commands.UpdateBrand
{
    public sealed class UpdateBrandCommandHandler : IRequestHandler<UpdateBrandCommand, Result<string>>
    {
        private readonly IRepository<Brand> _brandRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateBrandCommandHandler(IRepository<Brand> brandRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _brandRepository = brandRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<string>> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
        {
            var brand = await _brandRepository.GetByIdAsync(request.Id, cancellationToken);
            if (brand is null)
                return Result<string>.Failure("Güncellenecek marka bulunamadı.");

            if (brand.Name.ToLower() != request.Name.ToLower())
            {
                var isNameExists = await _brandRepository.AnyAsync(x => x.Name.ToLower() == request.Name.ToLower(), cancellationToken);
                if (isNameExists)
                    return Result<string>.Failure("Bu marka ismi zaten kullanımda.");
            }

            _mapper.Map(request, brand);
            _brandRepository.Update(brand);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(brand.Name, "Marka başarıyla güncellendi.");
        }
    }
}
