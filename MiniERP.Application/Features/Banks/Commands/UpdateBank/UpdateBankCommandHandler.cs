using AutoMapper;
using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Features.Banks.Commands.UpdateBank
{
    public sealed class UpdateBankCommandHandler : IRequestHandler<UpdateBankCommand, Result<string>>
    {
        private readonly IRepository<Bank> _bankRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateBankCommandHandler(IRepository<Bank> bankRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _bankRepository = bankRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<string>> Handle(UpdateBankCommand request, CancellationToken cancellationToken)
        {
            var bank = await _bankRepository.GetByIdAsync(request.Id, cancellationToken);
            if (bank is null) return Result<string>.Failure("Banka hesabı bulunamadı.");

            // IBAN kontrolü: Kendisi hariç başka bir kayıtta bu IBAN var mı?
            var isIbanExists = await _bankRepository.AnyAsync(x => x.IBAN == request.IBAN && x.Id != request.Id, cancellationToken);
            if (isIbanExists) return Result<string>.Failure("Bu IBAN başka bir banka hesabına kayıtlı.");

            _mapper.Map(request, bank);
            bank.UpdatedDate = DateTime.Now;

            _bankRepository.Update(bank);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(bank.AccountName, "Banka hesabı başarıyla güncellendi.");
        }
    }
}
