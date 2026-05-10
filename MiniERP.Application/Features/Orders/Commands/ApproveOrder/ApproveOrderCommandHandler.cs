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
            // Serializable isolation level mantıklı, stokta çakışma olmasın
            await using var transaction = await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

            try
            {
                var order = await _orderRepository.GetByIdWithIncludesAsync(
                    request.OrderId,
                    cancellationToken,
                    x => x.OrderDetails);

                if (order == null) return Result<string>.Failure("Sipariş bulunamadı.");
                if (order.IsDeleted) return Result<string>.Failure("Silinmiş sipariş onaylanamaz.");
                if (order.Status != OrderStatus.Pending) return Result<string>.Failure("Sipariş zaten onaylanmış veya iptal edilmiş.");

                foreach (var line in order.OrderDetails)
                {
                    // 🚀 1. FİZİKİ BAKİYE HESABI (Düzeltildi)
                    // Sadece Out (Çıkış) olanları eksi say, In ve Opening olanları artı say.
                    var physicalBalance = _stockRepository
                        .Where(x => x.ProductId == line.ProductId && x.WarehouseId == line.WarehouseId && !x.IsDeleted)
                        .Select(x => x.Type == StockTransactionType.Out ? -x.Quantity : x.Quantity)
                        .Sum(); // ToList() kaldırıldı, direkt SQL'de toplansın

                    // 🚀 2. REZERVE MİKTAR HESABI
                    // Onaylanmış ama henüz faturaya dönüşüp stoktan düşmemiş siparişler
                    var reservedAmount = _orderDetailRepository
                        .Where(x => x.ProductId == line.ProductId &&
                                    x.WarehouseId == line.WarehouseId &&
                                    x.Order!.Status == OrderStatus.Approved && // Sadece Approved olanlar rezerve sayılır
                                    !x.IsDeleted &&
                                    x.OrderId != order.Id)
                        .Select(x => x.Quantity)
                        .Sum();

                    var availableStock = physicalBalance - reservedAmount;

                    // 🚀 3. MÜSAİTLİK KONTROLÜ
                    if (availableStock < line.Quantity)
                    {
                        // Mesajı daha kullanıcı dostu yaptık
                        return Result<string>.Failure($"Yetersiz stok! Müsait: {availableStock:N2}, Talep: {line.Quantity:N2}. " +
                            $"(Fiziki: {physicalBalance:N2}, Rezerve: {reservedAmount:N2})");
                    }
                }

                // 2. ONAY VE KAYIT
                order.Approve(); // Burada Status = Approved yapıyorsun
                await _orderRepository.UpdateAsync(order, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Result<string>.Success(order.OrderNumber, "Sipariş onaylandı ve stok rezerve edildi.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<string>.Failure($"Beklenmedik bir hata: {ex.Message}");
            }
        }
    }
}