using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using CustomerEntity = MiniERP.Domain.Entities.Customer;
namespace MiniERP.Application.Features.Orders.Queries.GetOrderById
{
    public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<GetOrderByIdResponse>>
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<CustomerEntity> _customerRepository;
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IAppUserService _appUserService;

        public GetOrderByIdQueryHandler(IRepository<Order> orderRepository, IRepository<CustomerEntity> customerRepository, IRepository<Warehouse> warehouseRepository, IRepository<Product> productRepository, IAppUserService appUserService)
        {
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _warehouseRepository = warehouseRepository;
            _productRepository = productRepository;
            _appUserService = appUserService;
        }

        public async Task<Result<GetOrderByIdResponse>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdWithIncludesAsync(request.Id, cancellationToken, x => x.OrderDetails);
            if (order == null) return Result<GetOrderByIdResponse>.Failure("Sipariş bulunamadı.");

            // ID'leri Topla
            var productIds = order.OrderDetails.Select(x => x.ProductId).Distinct().ToList();
            var warehouseIds = order.OrderDetails.Select(x => x.WarehouseId).Distinct().ToList();
            warehouseIds.Add(order.WarehouseId);

            // Dictionary Çözümleri
            var productDict = (await _productRepository.ToListAsync(_productRepository.Where(x => productIds.Contains(x.Id)), cancellationToken)).ToDictionary(x => x.Id, x => x.Name);
            var warehouseDict = (await _warehouseRepository.ToListAsync(_warehouseRepository.Where(x => warehouseIds.Contains(x.Id)), cancellationToken)).ToDictionary(x => x.Id, x => x.Name);
            var customer = await _customerRepository.GetByIdAsync(order.CustomerId, cancellationToken);
            var userNames = await _appUserService.GetUserNamesByIdsAsync(new List<string?> { order.CreatedBy }, cancellationToken);

            // Map
            var response = new GetOrderByIdResponse(
                order.Id,
                order.OrderNumber,
                order.OrderDate,
                customer?.Name ?? "Tanımsız",
                warehouseDict.TryGetValue(order.WarehouseId, out var wName) ? wName : "Tanımsız Depo",
                order.Status.ToString(),
                userNames.TryGetValue(order.CreatedBy ?? "", out var uName) ? uName : "Sistem",
                order.OrderDetails.Select(d => new OrderLineResponse(
                    d.ProductId,
                    productDict.TryGetValue(d.ProductId, out var pName) ? pName : "Bilinmeyen Ürün",
                    d.Quantity,
                    d.UnitPrice,
                    warehouseDict.TryGetValue(d.WarehouseId, out var lineWName) ? lineWName : "Bilinmeyen Depo"
                )).ToList()
            );

            return Result<GetOrderByIdResponse>.Success(response,"Sipariş bilgisi başarıyla çekildi");
        }
    }
}
