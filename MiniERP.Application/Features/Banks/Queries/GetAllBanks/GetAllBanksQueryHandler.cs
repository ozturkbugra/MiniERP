using AutoMapper;
using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Features.Banks.Queries.GetAllBanks
{
    public sealed class GetAllBanksQueryHandler : IRequestHandler<GetAllBanksQuery, Result<List<BankResponse>>>
    {
        private readonly IRepository<Bank> _bankRepository;
        private readonly IMapper _mapper;

        public GetAllBanksQueryHandler(IRepository<Bank> bankRepository, IMapper mapper)
        {
            _bankRepository = bankRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<BankResponse>>> Handle(GetAllBanksQuery request, CancellationToken cancellationToken)
        {
            var banks = await _bankRepository.GetAllAsync(cancellationToken);
            var response = _mapper.Map<List<BankResponse>>(banks.OrderBy(x => x.BankName));

            return Result<List<BankResponse>>.Success(response, "Banka listesi başarıyla getirildi.");
        }
    }
}
