using AutoMapper;
using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Features.Banks.Queries.GetBankById
{
    public sealed class GetBankByIdQueryHandler : IRequestHandler<GetBankByIdQuery, Result<BankResponse>>
    {
        private readonly IRepository<Bank> _bankRepository;
        private readonly IMapper _mapper;

        public GetBankByIdQueryHandler(IRepository<Bank> bankRepository, IMapper mapper)
        {
            _bankRepository = bankRepository;
            _mapper = mapper;
        }

        public async Task<Result<BankResponse>> Handle(GetBankByIdQuery request, CancellationToken cancellationToken)
        {
            var bank = await _bankRepository.GetByIdAsync(request.Id, cancellationToken);
            if (bank is null) return Result<BankResponse>.Failure("Banka bulunamadı.");

            var response = _mapper.Map<BankResponse>(bank);
            return Result<BankResponse>.Success(response , "Banka bilgileri başarıyla getirildi.");
        }
    }
}
