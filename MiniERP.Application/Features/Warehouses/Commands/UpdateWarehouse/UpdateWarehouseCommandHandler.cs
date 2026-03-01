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

namespace MiniERP.Application.Features.Warehouses.Commands.UpdateWarehouse
{
    public sealed class UpdateWarehouseCommandHandler : IRequestHandler<UpdateWarehouseCommand, Result<string>>
    {
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateWarehouseCommandHandler(IRepository<Warehouse> warehouseRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _warehouseRepository = warehouseRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<string>> Handle(UpdateWarehouseCommand request, CancellationToken cancellationToken)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(request.Id, cancellationToken);
            if (warehouse is null)
                return Result<string>.Failure("Güncellenecek depo bulunamadı.");

            if (warehouse.Code != request.Code || warehouse.Name != request.Name)
            {
                var isAnyConflict = await _warehouseRepository
                    .AnyAsync(x => x.Id != request.Id && (x.Code == request.Code || x.Name == request.Name), cancellationToken);

                if (isAnyConflict)
                    return Result<string>.Failure("Bu depo kodu veya ismi zaten başka bir depoda tanımlı.");
            }

            _mapper.Map(request, warehouse);
            _warehouseRepository.Update(warehouse);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(warehouse.Name, "Depo bilgileri başarıyla güncellendi.");
        }
    }
}
