using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using CustomerEntity = MiniERP.Domain.Entities.Customer;

namespace MiniERP.Application.Features.Invoices.Queries.GetAllInvoices;

public sealed class GetInvoicesQueryHandler : IRequestHandler<GetInvoicesQuery, Result<List<GetInvoicesResponse>>>
{
    private readonly IRepository<Invoice> _invoiceRepository;
    private readonly IRepository<CustomerEntity> _customerRepository;
    private readonly IRepository<Warehouse> _warehouseRepository; // YENİ: Depolar için
    private readonly IAppUserService _appUserService;

    public GetInvoicesQueryHandler(
        IRepository<Invoice> invoiceRepository,
        IRepository<CustomerEntity> customerRepository,
        IAppUserService appUserService,
        IRepository<Warehouse> warehouseRepository)
    {
        _invoiceRepository = invoiceRepository;
        _customerRepository = customerRepository;
        _appUserService = appUserService;
        _warehouseRepository = warehouseRepository;
    }

    public async Task<Result<List<GetInvoicesResponse>>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        // 1. Faturaları ham halde çekiyoruz
        var invoices = await _invoiceRepository.GetAllAsync(cancellationToken);

        // 2. Müşteri İsimlerini Topluca Al
        var customerIds = invoices.Select(x => x.CustomerId).Distinct().ToList();


      var warehouseIds = invoices.Where(x => x.WarehouseId.HasValue)
                                   .Select(x => x.WarehouseId.Value)
                                   .Distinct()
                                   .ToList();
        var warehouseQuery = _warehouseRepository.Where(x => warehouseIds.Contains(x.Id));
        var warehouses = await _warehouseRepository.ToListAsync(warehouseQuery, cancellationToken);
        var warehouseDict = warehouses.ToDictionary(x => x.Id, x => x.Name);


        // IRepository üzerindeki Where ve ToListAsync kullanarak müşterileri çekiyoruz
        var customerQuery = _customerRepository.Where(x => customerIds.Contains(x.Id));
        var customers = await _customerRepository.ToListAsync(customerQuery, cancellationToken);
        var customerDict = customers.ToDictionary(x => x.Id, x => x.Name);

        // 3. Kullanıcı İsimlerini Topluca Al
        var userIds = invoices.Select(x => x.CreatedBy)
                              .Union(invoices.Select(x => x.UpdatedBy))
                              .Where(id => !string.IsNullOrEmpty(id))
                              .Distinct()
                              .ToList();

        var usersDictionary = await _appUserService.GetUserNamesByIdsAsync(userIds, cancellationToken);

        // 4. Saf ve hızlı manuel mapping
        var response = invoices.OrderByDescending(x => x.InvoiceDate).Select(invoice => new GetInvoicesResponse(
                         invoice.Id,                                     
                         invoice.InvoiceNumber,                          
                         invoice.InvoiceDate,                            
                         invoice.CustomerId,                             
                         customerDict.TryGetValue(invoice.CustomerId, out var cName) ? cName : "Tanımsız Müşteri", 
                         invoice.WarehouseId,                            
                         invoice.WarehouseId.HasValue && warehouseDict.TryGetValue(invoice.WarehouseId.Value, out var wName) ? wName : "Depo Belirtilmemiş", 
                         invoice.GrandTotal,                             
                         invoice.Status.ToString(),                      
                         invoice.CreatedBy != null && usersDictionary.TryGetValue(invoice.CreatedBy, out var createdName) ? createdName : "Sistem", 
                         invoice.UpdatedBy != null && usersDictionary.TryGetValue(invoice.UpdatedBy, out var updatedName) ? updatedName : null
         )).ToList();

        return Result<List<GetInvoicesResponse>>.Success(response, "Fatura listesi başarıyla getirildi.");
    }
}