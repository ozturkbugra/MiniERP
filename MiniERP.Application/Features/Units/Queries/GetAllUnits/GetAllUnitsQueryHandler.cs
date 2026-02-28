using AutoMapper;
using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using UnitEntity = MiniERP.Domain.Entities.Unit;

namespace MiniERP.Application.Features.Units.Queries.GetAllUnits
{
    public sealed class GetAllUnitsQueryHandler : IRequestHandler<GetAllUnitsQuery, Result<List<GetAllUnitsResponse>>>
    {
        private readonly IRepository<UnitEntity> _unitRepository;
        private readonly IMapper _mapper;

        public GetAllUnitsQueryHandler(IRepository<UnitEntity> unitRepository, IMapper mapper)
        {
            _unitRepository = unitRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<GetAllUnitsResponse>>> Handle(GetAllUnitsQuery request, CancellationToken cancellationToken)
        {
            var units = await _unitRepository.GetAllAsync(cancellationToken);
            var response = _mapper.Map<List<GetAllUnitsResponse>>(units);
            return Result<List<GetAllUnitsResponse>>.Success(response, "Birimler başarılı bir şekilde listelendi.");
        }
    }
}
