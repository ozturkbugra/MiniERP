using AutoMapper;
using MiniERP.Application.Features.Warehouses.Commands.CreateWarehouse;
using MiniERP.Application.Features.Warehouses.Commands.UpdateWarehouse;
using MiniERP.Application.Features.Warehouses.Queries.GetAllWarehouses;
using MiniERP.Application.Features.Warehouses.Queries.GetWarehouseById;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Mappings
{
    public sealed class WarehouseProfile : Profile
    {
        public WarehouseProfile()
        {
            CreateMap<CreateWarehouseCommand, Warehouse>();
            CreateMap<Warehouse, GetAllWarehousesResponse>();
            CreateMap<Warehouse, GetWarehouseByIdResponse>();
            CreateMap<UpdateWarehouseCommand, Warehouse>();
        }
    }
}
