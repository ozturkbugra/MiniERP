using AutoMapper;
using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using CustomerEntity = MiniERP.Domain.Entities.Customer;

namespace MiniERP.Application.Features.Customer.Queries.GetAllCustomers
{
    public sealed class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, Result<List<GetAllCustomersQueryResponse>>>
    {
        private readonly IRepository<CustomerEntity> _customerRepository;
        private readonly IMapper _mapper;

        public GetAllCustomersQueryHandler(IRepository<CustomerEntity> customerRepository, IMapper mapper)
        {
            _customerRepository = customerRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<GetAllCustomersQueryResponse>>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
        {
            var customers = await _customerRepository.GetAllAsync(cancellationToken);

            var orderedCustomers = customers.OrderBy(x => x.Name).ToList();

            var response = _mapper.Map<List<GetAllCustomersQueryResponse>>(orderedCustomers);

            return Result<List<GetAllCustomersQueryResponse>>.Success(response, "Cari listesi başarıyla getirildi.");
        }
    }
}
