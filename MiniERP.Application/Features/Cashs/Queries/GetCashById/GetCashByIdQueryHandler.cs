using AutoMapper;
using MediatR;
using MiniERP.Application.Features.Cashs.Queries.GetAllCashes;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using CashEntity = MiniERP.Domain.Entities.Cash; // Alias kullanımı

namespace MiniERP.Application.Features.Cashs.Queries.GetCashById
{
    public sealed class GetCashByIdQueryHandler : IRequestHandler<GetCashByIdQuery, Result<CashByIdResponse>>
    {
        private readonly IRepository<CashEntity> _cashRepository;
        private readonly IAppUserService _appUserService;

        public GetCashByIdQueryHandler(IRepository<CashEntity> cashRepository, IAppUserService appUserService)
        {
            _cashRepository = cashRepository;
            _appUserService = appUserService;
        }

        public async Task<Result<CashByIdResponse>> Handle(GetCashByIdQuery request, CancellationToken cancellationToken)
        {
            // 1. Veriyi çek
            var cash = await _cashRepository.GetByIdAsync(request.Id, cancellationToken);
            if (cash is null) return Result<CashByIdResponse>.Failure("Kasa bulunamadı.");

            // 2. ID listesini hazırla
            var ids = new List<string>();
            if (!string.IsNullOrEmpty(cash.CreatedBy)) ids.Add(cash.CreatedBy);
            if (!string.IsNullOrEmpty(cash.UpdatedBy)) ids.Add(cash.UpdatedBy);

            // 3. İsimleri çek
            var usersDictionary = await _appUserService.GetUserNamesByIdsAsync(ids.Distinct().ToList(), cancellationToken);

            // 4. MANUEL BİRLEŞTİRME
            string createdName = "Sistem";
            if (!string.IsNullOrEmpty(cash.CreatedBy))
            {
                usersDictionary.TryGetValue(cash.CreatedBy, out var name);
                createdName = name ?? "Sistem";
            }

            string? updatedName = null;
            if (!string.IsNullOrEmpty(cash.UpdatedBy))
            {
                usersDictionary.TryGetValue(cash.UpdatedBy, out var name);
                updatedName = name;
            }

            var response = new CashByIdResponse(
                cash.Id,
                cash.Name,
                cash.CurrencyType.ToString(),
                createdName,
                updatedName
            );

            return Result<CashByIdResponse>.Success(response, "Kasa bilgileri getirildi.");
        }
    }
}
