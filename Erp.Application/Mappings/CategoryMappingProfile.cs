using AutoMapper;
using Erp.Domain.DTOs.Category;
using Erp.Domain.Entities;

namespace Erp.Application.Mappings;

public class CategoryMappingProfile : Profile
{
    public CategoryMappingProfile()
    {
        CreateMap<Category, CategoryDto>();
        CreateMap<CategoryCreateDto, Category>();
        CreateMap<CategoryUpdateDto, Category>();
        CreateMap<Category, CategoryWithProductsDto>()
            .ForMember(o=> o.Products, o => o.MapFrom(o => o.ProductCategories.Select(o=>o.Product)));
	}
} 