using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Features.Banks.Queries.GetAllBanks;

public sealed class GetAllBanksQueryHandler : IRequestHandler<GetAllBanksQuery, Result<List<BankResponse>>>
{
    private readonly IRepository<Bank> _bankRepository;
    private readonly IAppUserService _appUserService;

    public GetAllBanksQueryHandler(IRepository<Bank> bankRepository, IAppUserService appUserService)
    {
        _bankRepository = bankRepository;
        _appUserService = appUserService;
    }

    public async Task<Result<List<BankResponse>>> Handle(GetAllBanksQuery request, CancellationToken cancellationToken)
    {
        var banks = await _bankRepository.GetAllAsync(cancellationToken);

        // N+1 yememek için ID'leri topluyoruz
        var userIds = banks.Select(x => x.CreatedBy)
                           .Union(banks.Select(x => x.UpdatedBy))
                           .Where(id => !string.IsNullOrEmpty(id))
                           .Distinct()
                           .ToList();

        // İsimleri veritabanından tek seferde alıyoruz
        var usersDictionary = await _appUserService.GetUserNamesByIdsAsync(userIds, cancellationToken);

        // AutoMapper YOK! Saf, hızlı ve %100 güvenli manuel eşleştirme:
        var response = banks.OrderBy(x => x.BankName).Select(bank => new BankResponse(
            bank.Id,
            bank.BankName,
            bank.AccountName,
            bank.IBAN,
            bank.BranchName,
            bank.CurrencyType.ToString(),
            bank.CreatedBy != null && usersDictionary.TryGetValue(bank.CreatedBy, out var createdName) ? createdName : "Sistem",
            bank.UpdatedBy != null && usersDictionary.TryGetValue(bank.UpdatedBy, out var updatedName) ? updatedName : null
        )).ToList();

        return Result<List<BankResponse>>.Success(response, "Banka listesi başarıyla getirildi.");
    }
}