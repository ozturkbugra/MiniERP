using AutoMapper;
using MiniERP.Application.Features.Customer.Commands.CreateCustomer;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Mappings
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            CreateMap<CreateCustomerCommand, Customer>();
        }
    }
}
