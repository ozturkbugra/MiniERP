using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Features.Banks.Commands.DeleteBank
{
    public sealed class DeleteBankCommandHandler : IRequestHandler<DeleteBankCommand, Result<string>>
    {
        private readonly IRepository<Bank> _bankRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteBankCommandHandler(IRepository<Bank> bankRepository, IUnitOfWork unitOfWork)
        {
            _bankRepository = bankRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<string>> Handle(DeleteBankCommand request, CancellationToken cancellationToken)
        {
            var bank = await _bankRepository.GetByIdAsync(request.Id, cancellationToken);

            if (bank is null) return Result<string>.Failure("Banka hesabı bulunamadı.");

            bank.IsDeleted = true; 
            bank.UpdatedDate = DateTime.Now;

            _bankRepository.Update(bank);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(bank.AccountName, "Banka hesabı başarıyla silindi.");
        }
    }
}
