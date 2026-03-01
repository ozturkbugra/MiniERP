using AutoMapper;
using MiniERP.Application.Features.Categories.Command.CreateCategory;
using MiniERP.Application.Features.Categories.Command.UpdateCategory;
using MiniERP.Application.Features.Categories.Queries.GetAllCategories;
using MiniERP.Application.Features.Categories.Queries.GetCategoryById;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Mappings
{
    public sealed class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<CreateCategoryCommand, Category>();
            CreateMap<Category, GetAllCategoriesResponse>();
            CreateMap<Category, GetCategoryByIdResponse>();
            CreateMap<UpdateCategoryCommand, Category>();
        }
    }
}
