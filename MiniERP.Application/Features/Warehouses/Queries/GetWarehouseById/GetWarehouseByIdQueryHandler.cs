using AutoMapper;
using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Features.Warehouses.Queries.GetWarehouseById
{
    public sealed class GetWarehouseByIdQueryHandler : IRequestHandler<GetWarehouseByIdQuery, Result<GetWarehouseByIdResponse>>
    {
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IMapper _mapper;

        public GetWarehouseByIdQueryHandler(IRepository<Warehouse> warehouseRepository, IMapper mapper)
        {
            _warehouseRepository = warehouseRepository;
            _mapper = mapper;
        }

        public async Task<Result<GetWarehouseByIdResponse>> Handle(GetWarehouseByIdQuery request, CancellationToken cancellationToken)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(request.Id, cancellationToken);

            if (warehouse is null)
                return Result<GetWarehouseByIdResponse>.Failure("Aradığınız depo sistemde bulunamadı.");

            var response = _mapper.Map<GetWarehouseByIdResponse>(warehouse);

            return Result<GetWarehouseByIdResponse>.Success(response, "Depo başarıyla getirildi.");
        }
    }
}
