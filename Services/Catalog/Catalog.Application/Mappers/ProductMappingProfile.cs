using AutoMapper;
using Catalog.Application.Commands;
using Catalog.Application.Responses;
using Catalog.Core.Entities;
using Catalog.Core.Specs;

namespace Catalog.Application.Mappers
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            CreateMap<ProductBrand, BrandResponse>().ReverseMap();
            //Use it in case of using _mapper DI
            //CreateMap<ProductBrand, BrandResponse>().
            //.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            //.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
            CreateMap<Product, ProductResponse>().ReverseMap();
            CreateMap<ProductType, TypeResponse>().ReverseMap();
            CreateMap<Product, CreateProductCommand>().ReverseMap();
            CreateMap<Pagination<Product>, Pagination<ProductResponse>>();
        }
    }
}
