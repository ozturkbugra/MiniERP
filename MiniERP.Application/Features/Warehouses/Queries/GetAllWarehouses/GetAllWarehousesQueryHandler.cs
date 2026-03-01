using AutoMapper;
using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.Warehouses.Queries.GetAllWarehouses
{
    public sealed class GetAllWarehousesQueryHandler : IRequestHandler<GetAllWarehousesQuery, Result<List<GetAllWarehousesResponse>>>
    {
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IMapper _mapper;

        public GetAllWarehousesQueryHandler(IRepository<Warehouse> warehouseRepository, IMapper mapper)
        {
            _warehouseRepository = warehouseRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<GetAllWarehousesResponse>>> Handle(GetAllWarehousesQuery request, CancellationToken cancellationToken)
        {
            var warehouses = await _warehouseRepository.GetAllAsync(cancellationToken);
            var response = _mapper.Map<List<GetAllWarehousesResponse>>(warehouses);

            return Result<List<GetAllWarehousesResponse>>.Success(response,"Depolar listelendi");
        }
    }
}
