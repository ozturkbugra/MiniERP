using AutoMapper;
using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using CustomerEntity = MiniERP.Domain.Entities.Customer;

namespace MiniERP.Application.Features.Customer.Commands.CreateCustomer
{
    public sealed class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<string>>
    {

        private readonly IMapper _mapper;
        private readonly IRepository<CustomerEntity> _customerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateCustomerCommandHandler(IRepository<CustomerEntity> customerRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }



        public async Task<Result<string>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            var isNameExists = await _customerRepository.AnyAsync(x => x.Name.ToLower() == request.Name.ToLower(), cancellationToken);

            if (isNameExists)
            {
                return Result<string>.Failure("Bu isimde bir cari kart zaten mevcut.");
            }

            var customer = _mapper.Map<CustomerEntity>(request);
            await _customerRepository.AddAsync(customer, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(customer.Name, "Cari kart başarıyla oluşturuldu.");
        }
    }
}
