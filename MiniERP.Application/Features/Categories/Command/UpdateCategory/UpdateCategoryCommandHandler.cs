using AutoMapper;
using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Features.Categories.Command.UpdateCategory
{
    public sealed class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result<string>>
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateCategoryCommandHandler(IRepository<Category> categoryRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<string>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (category is null)
                return Result<string>.Failure("Güncellenecek kategori bulunamadı.");

            if (category.Name.ToLower() != request.Name.ToLower())
            {
                var isNameExists = await _categoryRepository.AnyAsync(x => x.Name.ToLower() == request.Name.ToLower(), cancellationToken);
                if (isNameExists)
                    return Result<string>.Failure("Bu kategori ismi zaten kullanımda.");
            }

            _mapper.Map(request, category);
            _categoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(category.Name, "Kategori başarıyla güncellendi.");
        }
    }
}
