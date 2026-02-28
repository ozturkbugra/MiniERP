using AutoMapper;
using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using UnitEntity = MiniERP.Domain.Entities.Unit;

namespace MiniERP.Application.Features.Units.Queries.GetUnitById
{
    public sealed class GetUnitByIdQueryHandler : IRequestHandler<GetUnitByIdQuery, Result<GetUnitByIdResponse>>
    {
        private readonly IRepository<UnitEntity> _unitRepository;
        private readonly IMapper _mapper;

        public GetUnitByIdQueryHandler(IRepository<UnitEntity> unitRepository, IMapper mapper)
        {
            _unitRepository = unitRepository;
            _mapper = mapper;
        }

        public async Task<Result<GetUnitByIdResponse>> Handle(GetUnitByIdQuery request, CancellationToken cancellationToken)
        {
            var unit = await _unitRepository.GetByIdAsync(request.Id, cancellationToken);

            if (unit is null)
                return Result<GetUnitByIdResponse>.Failure("Aradığınız birim sistemde bulunamadı.");

            var response = _mapper.Map<GetUnitByIdResponse>(unit);

            return Result<GetUnitByIdResponse>.Success(response, "Birim başarıyla getirildi.");
        }
    }
}
