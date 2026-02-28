using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Features.Brands.Commands.DeleteBrand
{
    public sealed class DeleteBrandCommandHandler : IRequestHandler<DeleteBrandCommand, Result<string>>
    {
        private readonly IRepository<Brand> _brandRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteBrandCommandHandler(IRepository<Brand> brandRepository, IUnitOfWork unitOfWork)
        {
            _brandRepository = brandRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<string>> Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
        {
            var brand = await _brandRepository.GetByIdAsync(request.Id, cancellationToken);
            if (brand is null)
                return Result<string>.Failure("Silinecek marka bulunamadı.");

            brand.IsDeleted = true;

            _brandRepository.Update(brand);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(brand.Name, "Marka başarıyla silindi.");
        }
    }
}
