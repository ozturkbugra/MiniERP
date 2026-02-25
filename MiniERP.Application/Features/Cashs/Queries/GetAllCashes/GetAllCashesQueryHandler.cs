using AutoMapper;
using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using CashEntity = MiniERP.Domain.Entities.Cash;

namespace MiniERP.Application.Features.Cashs.Queries.GetAllCashes
{
    public sealed class GetAllCashesQueryHandler : IRequestHandler<GetAllCashesQuery, Result<List<CashResponse>>>
    {
        private readonly IRepository<CashEntity> _cashRepository;
        private readonly IAppUserService _appUserService;

        public GetAllCashesQueryHandler(IRepository<CashEntity> cashRepository, IAppUserService appUserService)
        {
            _cashRepository = cashRepository;
            _appUserService = appUserService;
        }

        public async Task<Result<List<CashResponse>>> Handle(GetAllCashesQuery request, CancellationToken cancellationToken)
        {
            // 1. Veriyi çek
            var cashes = await _cashRepository.GetAllAsync(cancellationToken);

            // 2. Kullanıcı ID'lerini topla (CreatedBy ve UpdatedBy)
            var userIds = cashes.Select(x => x.CreatedBy).Union(cashes.Select(x => x.UpdatedBy))
                               .Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();

            // 3. İsimleri Dictionary olarak al
            var usersDictionary = await _appUserService.GetUserNamesByIdsAsync(userIds, cancellationToken);

            // 4. MANUEL BİRLEŞTİRME (Hata almamak için güvenli yöntem)
            var response = cashes.OrderBy(x => x.Name).Select(cash =>
            {
                // CreatedBy eşleştirmesi
                string createdName = "Sistem";
                if (!string.IsNullOrEmpty(cash.CreatedBy))
                {
                    usersDictionary.TryGetValue(cash.CreatedBy, out var name);
                    createdName = name ?? "Sistem";
                }

                // UpdatedBy eşleştirmesi
                string? updatedName = null;
                if (!string.IsNullOrEmpty(cash.UpdatedBy))
                {
                    usersDictionary.TryGetValue(cash.UpdatedBy, out var name);
                    updatedName = name;
                }

                return new CashResponse(
                    cash.Id,
                    cash.Name,
                    cash.CurrencyType.ToString(),
                    createdName,
                    updatedName
                );
            }).ToList();

            return Result<List<CashResponse>>.Success(response, "Kasa listesi başarıyla getirildi.");
        }
    }
}
