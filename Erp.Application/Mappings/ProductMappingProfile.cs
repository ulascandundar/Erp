using AutoMapper;
using Erp.Domain.DTOs.Product;
using Erp.Domain.Entities;

namespace Erp.Application.Mappings;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductDto>();
        CreateMap<ProductCreateDto, Product>();
        CreateMap<ProductUpdateDto, Product>();
    }
} 