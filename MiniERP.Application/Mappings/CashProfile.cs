using AutoMapper;
using MiniERP.Application.Features.Cash.Commands.CreateCash;
using MiniERP.Application.Features.Cashs.Commands.UpdateCash;
using MiniERP.Application.Features.Cashs.Queries.GetAllCashes;
using MiniERP.Application.Features.Cashs.Queries.GetCashById;
using MiniERP.Application.Features.Customer.Queries.GetAllCustomers;
using MiniERP.Application.Features.Customer.Queries.GetCustomerById;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Mappings
{
    public class CashProfile : Profile
    {
        public CashProfile()
        {
            CreateMap<CreateCashCommand, Cash>();
            CreateMap<UpdateCashCommand, Cash>();
            CreateMap<Cash, CashResponse>()
                .ForMember(dest => dest.CurrencyType, opt => opt.MapFrom(src => src.CurrencyType.ToString()));
            CreateMap<Cash, CashByIdResponse>()
                .ForMember(dest => dest.CurrencyType, opt => opt.MapFrom(src => src.CurrencyType.ToString()));
        }
    }
}
