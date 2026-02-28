using AutoMapper;
using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Units.Commands.CreateUnit
{
    public sealed class CreateUnitCommandHandler : IRequestHandler<CreateUnitCommand, Result<string>>
    {
        private readonly IRepository<Domain.Entities.Unit> _unitRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateUnitCommandHandler(IRepository<Domain.Entities.Unit> unitRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitRepository = unitRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<string>> Handle(CreateUnitCommand request, CancellationToken cancellationToken)
        {
            var isCodeExists = await _unitRepository.AnyAsync(x => x.Code.ToLower() == request.Code.ToLower(), cancellationToken);
            if (isCodeExists) return Result<string>.Failure("Bu birim kodu zaten sistemde kayıtlı.");

            var unit = _mapper.Map<Domain.Entities.Unit>(request);
            await _unitRepository.AddAsync(unit, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(unit.Name, "Birim başarıyla oluşturuldu.");
        }
    }
}
