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
        private readonly IMapper _mapper;

        public GetAllCashesQueryHandler(IRepository<CashEntity> cashRepository, IMapper mapper)
        {
            _cashRepository = cashRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<CashResponse>>> Handle(GetAllCashesQuery request, CancellationToken cancellationToken)
        {
            var cashes = await _cashRepository.GetAllAsync(cancellationToken);

            var response = _mapper.Map<List<CashResponse>>(cashes.OrderBy(x => x.Name));

            return Result<List<CashResponse>>.Success(response, "Kasa listesi başarıyla getirildi.");
        }
    }
}
