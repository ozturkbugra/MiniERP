using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.Orders.Commands.CreateOrder
{
    public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<string>>
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateOrderCommandHandler(IRepository<Order> orderRepository, IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<string>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            // 1.Header oluşturuyoruz
            // Status otomatik olarak beklemede oluyor
            var order = new Order
            {
                OrderNumber = request.OrderNumber,
                OrderDate = request.OrderDate,
                Description = request.Description,
                Type = request.Type,
                CustomerId = request.CustomerId,
                WarehouseId = request.WarehouseId
            };

            // 2. Satırları (Lines) Header'a ekliyoruz
            foreach (var lineDto in request.OrderLines)
            {
                var line = new OrderDetail
                {
                    ProductId = lineDto.ProductId,
                    WarehouseId = lineDto.WarehouseId,
                    Quantity = lineDto.Quantity,
                    UnitPrice = lineDto.UnitPrice
                };

                order.OrderDetails.Add(line);
            }

            await _orderRepository.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(order.OrderNumber, "Sipariş taslak olarak başarıyla oluşturuldu.");
        }
    }
}
