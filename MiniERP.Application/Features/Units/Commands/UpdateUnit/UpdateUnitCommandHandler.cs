using AutoMapper;
using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using UnitEntity = MiniERP.Domain.Entities.Unit; 

namespace MiniERP.Application.Features.Units.Commands.UpdateUnit
{
    public sealed class UpdateUnitCommandHandler : IRequestHandler<UpdateUnitCommand, Result<string>>
    {
        private readonly IRepository<UnitEntity> _unitRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateUnitCommandHandler(IRepository<UnitEntity> unitRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitRepository = unitRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<string>> Handle(UpdateUnitCommand request, CancellationToken cancellationToken)
        {
            var unit = await _unitRepository.GetByIdAsync(request.Id, cancellationToken);
            if (unit is null)
                return Result<string>.Failure("Güncellenmek istenen birim sistemde bulunamadı.");

            if (unit.Code.ToLower() != request.Code.ToLower())
            {
                var isCodeExists = await _unitRepository.AnyAsync(x => x.Code.ToLower() == request.Code.ToLower(), cancellationToken);
                if (isCodeExists)
                    return Result<string>.Failure("Bu yeni birim kodu zaten başka bir kayıt tarafından kullanılıyor.");
            }

            _mapper.Map(request, unit);
            _unitRepository.Update(unit);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(unit.Name, "Birim başarıyla güncellendi.");
        }
    }
}
