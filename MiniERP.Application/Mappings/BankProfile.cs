using AutoMapper;
using MiniERP.Application.Features.Banks.Commands.CreateBank;
using MiniERP.Application.Features.Banks.Commands.UpdateBank;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Mappings
{
    public class BankProfile : Profile
    {
        public BankProfile()
        {
            CreateMap<CreateBankCommand, Bank>();
            CreateMap<UpdateBankCommand, Bank>();
        }
    }
}
