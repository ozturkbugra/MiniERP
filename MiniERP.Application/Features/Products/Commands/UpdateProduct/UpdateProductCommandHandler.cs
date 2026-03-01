using AutoMapper;
using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using UnitEntity = MiniERP.Domain.Entities.Unit;

namespace MiniERP.Application.Features.Products.Commands.UpdateProduct
{
    public sealed class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<string>>
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Brand> _brandRepository;
        private readonly IRepository<UnitEntity> _unitRepository;
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateProductCommandHandler(
            IRepository<Product> productRepository,
            IRepository<Category> categoryRepository,
            IRepository<Brand> brandRepository,
            IRepository<UnitEntity> unitRepository,
            IRepository<Warehouse> warehouseRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _unitRepository = unitRepository;
            _warehouseRepository = warehouseRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<string>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product is null)
                return Result<string>.Failure("Güncellenecek ürün bulunamadı.");

            // 1. Kod veya Barkod değişmişse benzersizlik kontrolü
            if (product.Code != request.Code || product.Barcode != request.Barcode)
            {
                var isConflict = await _productRepository.AnyAsync(x =>
                    x.Id != request.Id &&
                    (x.Code.ToLower() == request.Code.ToLower() || x.Barcode.ToLower() == request.Barcode.ToLower()),
                    cancellationToken);

                if (isConflict)
                    return Result<string>.Failure("Bu ürün kodu veya barkodu zaten başka bir üründe tanımlı.");
            }

            // 2. Yeni seçilen ID'lerin geçerlilik kontrolü
            if (!await _categoryRepository.AnyAsync(x => x.Id == request.CategoryId, cancellationToken))
                return Result<string>.Failure("Seçilen kategori geçersiz.");

            if (!await _brandRepository.AnyAsync(x => x.Id == request.BrandId, cancellationToken))
                return Result<string>.Failure("Seçilen marka geçersiz.");

            if (!await _unitRepository.AnyAsync(x => x.Id == request.UnitId, cancellationToken))
                return Result<string>.Failure("Seçilen birim geçersiz.");

            if (!await _warehouseRepository.AnyAsync(x => x.Id == request.WarehouseId, cancellationToken))
                return Result<string>.Failure("Seçilen depo geçersiz.");

            // 3. Mapping ve Kayıt
            _mapper.Map(request, product);
            _productRepository.Update(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(product.Name, "Ürün başarıyla güncellendi.");
        }
    }
}
