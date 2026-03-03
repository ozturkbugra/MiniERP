using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using MiniERP.Domain.Enums;
using System.Data;

namespace MiniERP.Application.Features.Orders.Commands.ApproveOrder
{
    public sealed class ApproveOrderCommandHandler : IRequestHandler<ApproveOrderCommand, Result<string>>
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderDetail> _orderDetailRepository;
        private readonly IRepository<StockTransaction> _stockRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ApproveOrderCommandHandler(
            IRepository<Order> orderRepository,
            IRepository<OrderDetail> orderDetailRepository,
            IRepository<StockTransaction> stockRepository,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _stockRepository = stockRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<string>> Handle(ApproveOrderCommand request, CancellationToken cancellationToken)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

            try
            {
                // Siparişi satırlarıyla beraber çekiyoruz.
                var order = await _orderRepository.GetByIdWithIncludesAsync(
                    request.OrderId,
                    cancellationToken,
                    x => x.OrderDetails);

                if (order == null) return Result<string>.Failure("Sipariş bulunamadı.");
                if (order.IsDeleted) return Result<string>.Failure("Silinmiş sipariş onaylanamaz.");
                if (order.Status != OrderStatus.Pending) return Result<string>.Failure("Sipariş zaten onaylanmış veya iptal edilmiş.");

                // STOK MÜSAİTLİK KONTROLÜ
                foreach (var line in order.OrderDetails)
                {
                    // a) Fiziki Bakiye
                    var physicalBalance = _stockRepository
                        .Where(x => x.ProductId == line.ProductId && x.WarehouseId == line.WarehouseId && !x.IsDeleted)
                        .AsEnumerable()
                        .Sum(x => x.Type == StockTransactionType.In ? x.Quantity : -x.Quantity);

                    // b) Rezerve Miktar
                    var reservedAmount = _orderDetailRepository
                        .Where(x => x.ProductId == line.ProductId &&
                                    x.WarehouseId == line.WarehouseId &&
                                    x.Order!.Status == OrderStatus.Approved &&
                                    !x.IsDeleted &&
                                    x.OrderId != order.Id)
                        .AsEnumerable() // Senkron kilitlenmeyi önlemek için buraya da AsEnumerable ekledik.
                        .Sum(x => x.Quantity);

                    var availableStock = physicalBalance - reservedAmount;

                    if (availableStock < line.Quantity)
                    {
                        // ürün yoksa işlemi geri sarıyoruz
                        return Result<string>.Failure($"Yetersiz stok! Ürün ID: {line.ProductId}, Müsait: {availableStock}, Talep: {line.Quantity}");
                    }
                }

                order.Approve();
                await _orderRepository.UpdateAsync(order, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Result<string>.Success(order.OrderNumber, "Sipariş onaylandı ve stok rezerve edildi.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<string>.Failure($"Beklenmedik bir hata oluştu: {ex.Message}");
            }
        }
    }
}