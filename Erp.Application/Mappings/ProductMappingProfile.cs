using AutoMapper;
using Erp.Domain.DTOs.Product;
using Erp.Domain.Entities;

namespace Erp.Application.Mappings;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.ProductCategories.Select(pc => pc.Category)));
        CreateMap<ProductCreateDto, Product>()
            .ForMember(dest => dest.ProductCategories, opt => opt.MapFrom(src => src.CategoryIds.Select(c => new ProductCategory { CategoryId = c })));
        CreateMap<ProductUpdateDto, Product>()
			.ForMember(dest => dest.ProductCategories, opt => opt.MapFrom(src => src.CategoryIds.Select(c => new ProductCategory { CategoryId = c })));
    }
} 