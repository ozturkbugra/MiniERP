using AutoMapper;
using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using CustomerEntity = MiniERP.Domain.Entities.Customer;

namespace MiniERP.Application.Features.Customer.Queries.GetCustomerById
{
    public sealed class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, Result<GetCustomerByIdQueryResponse>>
    {
        private readonly IRepository<CustomerEntity> _customerRepository;
        private readonly IMapper _mapper;

        public GetCustomerByIdQueryHandler(IRepository<CustomerEntity> customerRepository, IMapper mapper)
        {
            _customerRepository = customerRepository;
            _mapper = mapper;
        }

        public async Task<Result<GetCustomerByIdQueryResponse>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            // Repository üzerinden ID ile buluyoruz (Global filter burada da devrede!)
            var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);

            if (customer is null)
            {
                return Result<GetCustomerByIdQueryResponse>.Failure("Cari kart bulunamadı.");
            }

            var response = _mapper.Map<GetCustomerByIdQueryResponse>(customer);

            return Result<GetCustomerByIdQueryResponse>.Success(response, "Cari bilgileri başarıyla getirildi.");
        }
    }
}
