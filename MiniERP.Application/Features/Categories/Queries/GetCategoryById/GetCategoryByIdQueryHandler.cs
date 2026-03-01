using AutoMapper;
using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Features.Categories.Queries.GetCategoryById
{
    public sealed class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, Result<GetCategoryByIdResponse>>
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly IMapper _mapper;

        public GetCategoryByIdQueryHandler(IRepository<Category> categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<Result<GetCategoryByIdResponse>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);

            if (category is null)
                return Result<GetCategoryByIdResponse>.Failure("Kategori bulunamadı.");

            var response = _mapper.Map<GetCategoryByIdResponse>(category);

            return Result<GetCategoryByIdResponse>.Success(response, "Kategori başarıyla getirildi.");
        }
    }
}
