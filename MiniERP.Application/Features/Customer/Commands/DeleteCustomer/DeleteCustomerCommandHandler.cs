using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using CustomerEntity = MiniERP.Domain.Entities.Customer;


namespace MiniERP.Application.Features.Customer.Commands.DeleteCustomer
{
    public sealed class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, Result<string>>
    {
        private readonly IRepository<CustomerEntity> _customerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCustomerCommandHandler(IRepository<CustomerEntity> customerRepository, IUnitOfWork unitOfWork)
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<string>> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);

            if (customer is null)
            {
                return Result<string>.Failure("Silinecek cari kart bulunamadı.");
            }
          
            customer.IsDeleted = true;
            customer.UpdatedDate = DateTime.Now; 

            _customerRepository.Update(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(customer.Name, "Cari kart başarıyla silindi.");
        }
    }
}
