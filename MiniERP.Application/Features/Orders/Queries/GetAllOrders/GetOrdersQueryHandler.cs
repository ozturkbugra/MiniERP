using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using CustomerEntity = MiniERP.Domain.Entities.Customer;
namespace MiniERP.Application.Features.Orders.Queries.GetAllOrders
{
   public sealed class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, Result<List<GetOrdersResponse>>>
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<CustomerEntity> _customerRepository;
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IAppUserService _appUserService;

        public GetOrdersQueryHandler(
            IRepository<Order> orderRepository,
            IRepository<CustomerEntity> customerRepository,
            IRepository<Warehouse> warehouseRepository,
            IAppUserService appUserService)
        {
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _warehouseRepository = warehouseRepository;
            _appUserService = appUserService;
        }

        public async Task<Result<List<GetOrdersResponse>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
        {
            // 1. Tüm siparişleri tek seferde çekiyoruz
            var orders = await _orderRepository.GetAllAsync(cancellationToken);

            // 2. Müşteri İsimleri
            var customerIds = orders.Select(x => x.CustomerId).Distinct().ToList();
            var customerQuery = _customerRepository.Where(x => customerIds.Contains(x.Id));
            var customers = await _customerRepository.ToListAsync(customerQuery, cancellationToken);
            var customerDict = customers.ToDictionary(x => x.Id, x => x.Name);

            // 3. Depo İsimleri
            var warehouseIds = orders.Select(x => x.WarehouseId).Distinct().ToList();
            var warehouseQuery = _warehouseRepository.Where(x => warehouseIds.Contains(x.Id));
            var warehouses = await _warehouseRepository.ToListAsync(warehouseQuery, cancellationToken);
            var warehouseDict = warehouses.ToDictionary(x => x.Id, x => x.Name);

            // 4. Kullanıcı İsimleri
            var userIds = orders.Select(x => x.CreatedBy)
                                .Union(orders.Select(x => x.UpdatedBy))
                                .Where(id => !string.IsNullOrEmpty(id))
                                .Distinct()
                                .ToList();
            var usersDictionary = await _appUserService.GetUserNamesByIdsAsync(userIds, cancellationToken);

            // 5. Manuel Mapping 
            var response = orders.OrderByDescending(x => x.OrderDate).Select(order => new GetOrdersResponse(
                order.Id,                                    
                order.OrderNumber,                            
                order.OrderDate,                              
                order.CustomerId,                             
                customerDict.TryGetValue(order.CustomerId, out var cName) ? cName : "Tanımsız Müşteri",
                order.Status.ToString(),                      
                order.WarehouseId,                          
                warehouseDict.TryGetValue(order.WarehouseId, out var wName) ? wName : "Tanımsız Depo", 
                order.CreatedBy != null && usersDictionary.TryGetValue(order.CreatedBy, out var createdName) ? createdName : "Sistem", 
                order.UpdatedBy != null && usersDictionary.TryGetValue(order.UpdatedBy, out var updatedName) ? updatedName : null 
            )).ToList();

            return Result<List<GetOrdersResponse>>.Success(response, "Sipariş listesi başarıyla getirildi.");
        }
    }
}
