using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Features.Warehouses.Commands.DeleteWarehouse
{
    public sealed class DeleteWarehouseCommandHandler : IRequestHandler<DeleteWarehouseCommand, Result<string>>
    {
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteWarehouseCommandHandler(IRepository<Warehouse> warehouseRepository, IUnitOfWork unitOfWork)
        {
            _warehouseRepository = warehouseRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<string>> Handle(DeleteWarehouseCommand request, CancellationToken cancellationToken)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(request.Id, cancellationToken);

            if (warehouse is null)
                return Result<string>.Failure("Silinecek depo bulunamadı.");

            warehouse.IsDeleted = true;

            _warehouseRepository.Update(warehouse);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(warehouse.Name, "Depo başarıyla silindi.");
        }
    }
}
