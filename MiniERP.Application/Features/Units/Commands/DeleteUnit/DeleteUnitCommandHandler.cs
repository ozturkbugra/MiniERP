using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using UnitEntity = MiniERP.Domain.Entities.Unit;

namespace MiniERP.Application.Features.Units.Commands.DeleteUnit
{
    public sealed class DeleteUnitCommandHandler : IRequestHandler<DeleteUnitCommand, Result<string>>
    {
        private readonly IRepository<UnitEntity> _unitRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteUnitCommandHandler(IRepository<UnitEntity> unitRepository, IUnitOfWork unitOfWork)
        {
            _unitRepository = unitRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<string>> Handle(DeleteUnitCommand request, CancellationToken cancellationToken)
        {
            var unit = await _unitRepository.GetByIdAsync(request.Id, cancellationToken);
            if (unit is null)
                return Result<string>.Failure("Silinecek birim bulunamadı.");

            unit.IsDeleted = true;

            _unitRepository.Update(unit);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(unit.Name, "Birim başarıyla silindi (Soft Delete).");
        }
    }
}
