using AutoMapper;
using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Features.Categories.Queries.GetAllCategories
{
    public sealed class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, Result<List<GetAllCategoriesResponse>>>
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly IMapper _mapper;

        public GetAllCategoriesQueryHandler(IRepository<Category> categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<GetAllCategoriesResponse>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = await _categoryRepository.GetAllAsync(cancellationToken);
            var response = _mapper.Map<List<GetAllCategoriesResponse>>(categories);

            return Result<List<GetAllCategoriesResponse>>.Success(response,"Kategoriler başarıyla listelendi.");
        }
    }
}
