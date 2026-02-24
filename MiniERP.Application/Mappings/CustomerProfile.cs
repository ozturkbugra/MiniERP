using AutoMapper;
using MiniERP.Application.Features.Customer.Commands.CreateCustomer;
using MiniERP.Application.Features.Customer.Commands.UpdateCustomer;
using MiniERP.Application.Features.Customer.Queries.GetAllCustomers;
using MiniERP.Application.Features.Customer.Queries.GetCustomerById;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Mappings
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            CreateMap<CreateCustomerCommand, Customer>();
            CreateMap<UpdateCustomerCommand, Customer>();
            CreateMap<Customer, GetAllCustomersQueryResponse>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));
            CreateMap<Customer, GetCustomerByIdQueryResponse>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));
        }
    }
}
