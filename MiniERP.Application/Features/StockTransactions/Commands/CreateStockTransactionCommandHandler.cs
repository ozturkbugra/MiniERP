using AutoMapper;
using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using CustomerEntity = MiniERP.Domain.Entities.Customer;
using MiniERP.Domain.Enums;

namespace MiniERP.Application.Features.StockTransactions.Commands
{
    public sealed class CreateStockTransactionCommandHandler : IRequestHandler<CreateStockTransactionCommand, Result<string>>
    {
        private readonly IRepository<StockTransaction> _transactionRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IRepository<CustomerEntity> _customerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateStockTransactionCommandHandler(
            IRepository<StockTransaction> transactionRepository,
            IRepository<Product> productRepository,
            IRepository<Warehouse> warehouseRepository,
            IRepository<CustomerEntity> customerRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _transactionRepository = transactionRepository;
            _productRepository = productRepository;
            _warehouseRepository = warehouseRepository;
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<string>> Handle(CreateStockTransactionCommand request, CancellationToken cancellationToken)
        {
            // 1. Temel Miktar Kontrolü
            if (request.Quantity <= 0)
                return Result<string>.Failure("Miktar sıfırdan büyük olmalıdır.");

            // 2. Varlık Kontrolleri
            if (!await _productRepository.AnyAsync(x => x.Id == request.ProductId, cancellationToken))
                return Result<string>.Failure("Seçilen ürün geçersiz.");

            if (!await _warehouseRepository.AnyAsync(x => x.Id == request.WarehouseId, cancellationToken))
                return Result<string>.Failure("Seçilen depo geçersiz.");

            if (!await _customerRepository.AnyAsync(x => x.Id == request.CustomerId, cancellationToken))
                return Result<string>.Failure("Seçilen cari (müşteri/tedarikçi) geçersiz.");

            // 3. Kayıt
            var transaction = _mapper.Map<StockTransaction>(request);

            await _transactionRepository.AddAsync(transaction, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            string direction = request.Type == StockTransactionType.In ? "Girişi" : "Çıkışı";
            return Result<string>.Success(transaction.DocumentNo, $"Stok {direction} başarıyla kaydedildi.");
        }
    }
}
