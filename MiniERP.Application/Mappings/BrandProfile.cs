using AutoMapper;
using MiniERP.Application.Features.Brands.Commands.CreateBrand;
using MiniERP.Application.Features.Brands.Commands.UpdateBrand;
using MiniERP.Application.Features.Brands.Queries.GetAllBrands;
using MiniERP.Application.Features.Brands.Queries.GetBrandById;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Mappings
{
    public sealed class BrandProfile : Profile
    {
        public BrandProfile()
        {
            CreateMap<CreateBrandCommand, Brand>();
            CreateMap<Brand, GetAllBrandsResponse>();
            CreateMap<Brand, GetBrandByIdResponse>();
            CreateMap<UpdateBrandCommand, Brand>();
        }
    }
}
