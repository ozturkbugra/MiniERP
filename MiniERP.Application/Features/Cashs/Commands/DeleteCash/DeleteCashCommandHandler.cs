using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using CashEntity = MiniERP.Domain.Entities.Cash;

namespace MiniERP.Application.Features.Cashs.Commands.DeleteCash
{
    public sealed class DeleteCashCommandHandler : IRequestHandler<DeleteCashCommand, Result<string>>
    {
        private readonly IRepository<CashEntity> _cashRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCashCommandHandler(IRepository<CashEntity> cashRepository, IUnitOfWork unitOfWork)
        {
            _cashRepository = cashRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<string>> Handle(DeleteCashCommand request, CancellationToken cancellationToken)
        {
            var cash = await _cashRepository.GetByIdAsync(request.Id, cancellationToken);
            if (cash is null) return Result<string>.Failure("Silinecek kasa bulunamadı.");

            cash.IsDeleted = true;
            cash.UpdatedDate = DateTime.Now;

            _cashRepository.Update(cash);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(cash.Name, "Kasa silindi.");
        }
    }
}
