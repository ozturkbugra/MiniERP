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
        private readonly IMapper _mapper;

        public GetCashByIdQueryHandler(IRepository<CashEntity> cashRepository, IMapper mapper)
        {
            _cashRepository = cashRepository;
            _mapper = mapper;
        }

        public async Task<Result<CashByIdResponse>> Handle(GetCashByIdQuery request, CancellationToken cancellationToken)
        {
            var cash = await _cashRepository.GetByIdAsync(request.Id, cancellationToken);

            if (cash is null)
            {
                return Result<CashByIdResponse>.Failure("Kasa bulunamadı.");
            }

            var response = _mapper.Map<CashByIdResponse>(cash);

            return Result<CashByIdResponse>.Success(response, "Kasa bilgileri getirildi.");
        }
    }
}
