using AutoMapper;
using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using CustomerEntity = MiniERP.Domain.Entities.Customer;

namespace MiniERP.Application.Features.Customer.Commands.UpdateCustomer
{
    public sealed class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Result<string>>
    {
        private readonly IMapper _mapper;
        private readonly IRepository<CustomerEntity> _customerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCustomerCommandHandler(IMapper mapper, IRepository<CustomerEntity> customerRepository, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<string>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);
            if (customer is null) return Result<string>.Failure("Güncellenecek cari kart bulunamadı.");

            var isNameExists = await _customerRepository.AnyAsync(x =>
                x.Name.ToLower() == request.Name.ToLower() && x.Id != request.Id, cancellationToken);

            if (isNameExists) return Result<string>.Failure("Bu isimde başka bir cari kart zaten mevcut.");

            _mapper.Map(request, customer);
            customer.UpdatedDate = DateTime.Now; 

            _customerRepository.Update(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(customer.Name, "Cari kart başarıyla güncellendi.");
        }
    }
}
