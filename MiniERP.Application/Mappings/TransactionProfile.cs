using AutoMapper;
using MiniERP.Application.Features.Transactions.Commands.CreateCollection;
using MiniERP.Application.Features.Transactions.Commands.MakePayment;
using MiniERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Mappings
{
    public class TransactionProfile : Profile
    {
        public TransactionProfile()
        {
            // 1. Command -> CustomerTransaction (Müşteriden para çıkıyor: Credit)
            CreateMap<CreateCollectionCommand, CustomerTransaction>()
                .ForMember(dest => dest.Credit, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.Debit, opt => opt.MapFrom(src => 0));

            // 2. Command -> CashTransaction (Kasaya para giriyor: Debit)
            CreateMap<CreateCollectionCommand, CashTransaction>()
                .ForMember(dest => dest.Debit, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.Credit, opt => opt.MapFrom(src => 0));

            // 3. Command -> BankTransaction (Bankaya para giriyor: Debit)
            CreateMap<CreateCollectionCommand, BankTransaction>()
                .ForMember(dest => dest.Debit, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.Credit, opt => opt.MapFrom(src => 0));

            // 1. Command -> CustomerTransaction (Ödeme yapıyoruz: Debit)
            CreateMap<MakePaymentCommand, CustomerTransaction>()
                .ForMember(dest => dest.Debit, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.Credit, opt => opt.MapFrom(src => 0));

            // 2. Command -> Cash/Bank (Para çıkıyor: Credit)
            CreateMap<MakePaymentCommand, CashTransaction>()
                .ForMember(dest => dest.Credit, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.Debit, opt => opt.MapFrom(src => 0));

            CreateMap<MakePaymentCommand, BankTransaction>()
                .ForMember(dest => dest.Credit, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.Debit, opt => opt.MapFrom(src => 0));
        }
    }
}
