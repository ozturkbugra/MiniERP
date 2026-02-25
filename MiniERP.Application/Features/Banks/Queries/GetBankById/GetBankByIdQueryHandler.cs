using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Features.Banks.Queries.GetBankById
{
    public sealed class GetBankByIdQueryHandler : IRequestHandler<GetBankByIdQuery, Result<BankResponse>>
    {
        private readonly IRepository<Bank> _bankRepository;
        private readonly IAppUserService _appUserService;

        public GetBankByIdQueryHandler(IRepository<Bank> bankRepository, IAppUserService appUserService)
        {
            _bankRepository = bankRepository;
            _appUserService = appUserService;
        }

        public async Task<Result<BankResponse>> Handle(GetBankByIdQuery request, CancellationToken cancellationToken)
        {
            var bank = await _bankRepository.GetByIdAsync(request.Id, cancellationToken);
            if (bank is null) return Result<BankResponse>.Failure("Banka bulunamadı.");

            // 1. Sadece bu bankaya ait kullanıcı ID'lerini topluyoruz
            var userIds = new List<string>();
            if (!string.IsNullOrEmpty(bank.CreatedBy)) userIds.Add(bank.CreatedBy);
            if (!string.IsNullOrEmpty(bank.UpdatedBy)) userIds.Add(bank.UpdatedBy);

            // 2. İsimleri Dictionary olarak alıyoruz
            var usersDictionary = await _appUserService.GetUserNamesByIdsAsync(userIds.Distinct().ToList(), cancellationToken);

            // 3. AutoMapper YOK! Manuel, hızlı ve hatasız eşleştirme:
            var response = new BankResponse(
                bank.Id,
                bank.BankName,
                bank.AccountName,
                bank.IBAN,
                bank.BranchName,
                bank.CurrencyType.ToString(),
                bank.CreatedBy != null && usersDictionary.TryGetValue(bank.CreatedBy, out var createdName) ? createdName : "Sistem",
                bank.UpdatedBy != null && usersDictionary.TryGetValue(bank.UpdatedBy, out var updatedName) ? updatedName : null
            );

            return Result<BankResponse>.Success(response, "Banka bilgileri başarıyla getirildi.");
        }
    }
}