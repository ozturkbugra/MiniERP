using AutoMapper;
using MiniERP.Application.Features.Units.Commands.CreateUnit;
using MiniERP.Application.Features.Units.Commands.UpdateUnit;
using MiniERP.Application.Features.Units.Queries.GetAllUnits;
using MiniERP.Application.Features.Units.Queries.GetUnitById;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Mappings
{
    public class UnitProfile : Profile
    {
        public UnitProfile()
        {
            CreateMap<CreateUnitCommand, Unit>();
            CreateMap<UpdateUnitCommand, Unit>();
            CreateMap<Unit, GetAllUnitsResponse>();
            CreateMap<Unit, GetUnitByIdResponse>();

        }
    }
}
