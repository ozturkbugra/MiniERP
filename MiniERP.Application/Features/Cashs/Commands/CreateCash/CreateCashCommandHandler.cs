using AutoMapper;
using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using CashEntity = MiniERP.Domain.Entities.Cash;

namespace MiniERP.Application.Features.Cash.Commands.CreateCash
{
    public sealed class CreateCashCommandHandler : IRequestHandler<CreateCashCommand, Result<string>>
    {
        private readonly IRepository<CashEntity> _cashRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateCashCommandHandler(IRepository<CashEntity> cashRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _cashRepository = cashRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<string>> Handle(CreateCashCommand request, CancellationToken cancellationToken)
        {
            var isNameExists = await _cashRepository.AnyAsync(x => x.Name.ToLower() == request.Name.ToLower(), cancellationToken);
            if (isNameExists) return Result<string>.Failure("Bu isimde bir kasa zaten mevcut.");

            var cash = _mapper.Map<CashEntity>(request);
            await _cashRepository.AddAsync(cash, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(cash.Name, "Kasa başarıyla oluşturuldu.");
        }
    }
}
