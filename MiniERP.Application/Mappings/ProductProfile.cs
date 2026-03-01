using AutoMapper;
using MiniERP.Application.Features.Products.Commands.CreateProduct;
using MiniERP.Application.Features.Products.Commands.UpdateProduct;
using MiniERP.Application.Features.Products.Queries.GetAllProducts;
using MiniERP.Application.Features.Products.Queries.GetProductById;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Mappings
{
    public sealed class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<CreateProductCommand, Product>();
            CreateMap<UpdateProductCommand, Product>();

            CreateMap<Product, GetAllProductsResponse>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.Name))
            .ForMember(dest => dest.UnitName, opt => opt.MapFrom(src => src.Unit.Name));


            CreateMap<Product, GetProductByIdResponse>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.Name))
            .ForMember(dest => dest.UnitName, opt => opt.MapFrom(src => src.Unit.Name));
        }
    }
}
