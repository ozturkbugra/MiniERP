using AutoMapper;
using MiniERP.Application.Features.StockTransactions.Commands.CreateStockTransaction;
using MiniERP.Application.Features.StockTransactions.Queries.GetAllStockTransactions;
using MiniERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Mappings
{
    public sealed class StockTransactionProfile : Profile
    {
        public StockTransactionProfile()
        {
            // Ekleme Mapping'i
            CreateMap<CreateStockTransactionCommand, StockTransaction>();

            CreateMap<StockTransaction, GetAllStockTransactionsResponse>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src =>
                src.Product != null ? src.Product.Name : "Tanımsız Ürün"))

            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src =>
                src.Warehouse != null ? src.Warehouse.Name : "Tanımsız Depo"))

            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src =>
                src.Customer != null ? src.Customer.Name : "Tanımsız Cari"))

            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src =>
                src.Type.ToString()))

            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src =>
                src.Quantity * src.UnitPrice));
        }
    }
}
