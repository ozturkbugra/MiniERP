using AutoMapper;
using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Features.Warehouses.Commands.CreateWarehouse
{
    public sealed class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, Result<string>>
    {
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateWarehouseCommandHandler(IRepository<Warehouse> warehouseRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _warehouseRepository = warehouseRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<string>> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
        {
            var isExist = await _warehouseRepository.AnyAsync(x =>
                x.Code.ToLower() == request.Code.ToLower() ||
                x.Name.ToLower() == request.Name.ToLower(), cancellationToken);

            if (isExist)
                return Result<string>.Failure("Bu depo kodu veya ismi zaten sistemde kayıtlı.");

            var warehouse = _mapper.Map<Warehouse>(request);

            await _warehouseRepository.AddAsync(warehouse, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(warehouse.Name, "Depo başarıyla oluşturuldu.");
        }
    }
}
