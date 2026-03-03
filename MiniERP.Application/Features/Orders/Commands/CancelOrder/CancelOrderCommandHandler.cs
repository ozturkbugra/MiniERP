using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using MiniERP.Domain.Enums;

namespace MiniERP.Application.Features.Orders.Commands.CancelOrder
{
    public sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result<string>>
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CancelOrderCommandHandler(IRepository<Order> orderRepository, IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<string>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

            if (order == null) return Result<string>.Failure("Sipariş bulunamadı.");

            // Eğer sipariş zaten faturalanmışsa iptal edilememeli (ileride ekleyeceğiz)
            if (order.Status == OrderStatus.Canceled) return Result<string>.Failure("Sipariş zaten iptal edilmiş.");

            // Siparişi iptal durumuna çekiyoruz (3)
            order.Cancel();

            await _orderRepository.UpdateAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(order.OrderNumber, "Sipariş iptal edildi ve rezerve stoklar serbest bırakıldı.");
        }
    }
}
