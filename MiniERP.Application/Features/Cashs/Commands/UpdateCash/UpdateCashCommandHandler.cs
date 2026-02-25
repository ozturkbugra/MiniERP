using AutoMapper;
using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using CashEntity = MiniERP.Domain.Entities.Cash;

namespace MiniERP.Application.Features.Cashs.Commands.UpdateCash
{
    public sealed class UpdateCashCommandHandler : IRequestHandler<UpdateCashCommand, Result<string>>
    {
        private readonly IRepository<CashEntity> _cashRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateCashCommandHandler(IRepository<CashEntity> cashRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _cashRepository = cashRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<string>> Handle(UpdateCashCommand request, CancellationToken cancellationToken)
        {
            var cash = await _cashRepository.GetByIdAsync(request.Id, cancellationToken);
            if (cash is null) return Result<string>.Failure("Kasa bulunamadı.");

            var isNameExists = await _cashRepository.AnyAsync(x => x.Name.ToLower() == request.Name.ToLower() && x.Id != request.Id, cancellationToken);
            if (isNameExists) return Result<string>.Failure("Bu isimde başka bir kasa zaten mevcut.");

            _mapper.Map(request, cash);
            cash.UpdatedDate = DateTime.Now;

            _cashRepository.Update(cash);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(cash.Name, "Kasa başarıyla güncellendi.");
        }
    }
}
