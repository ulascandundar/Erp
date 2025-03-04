using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Erp.Domain.DTOs.ProductCategory;
using Erp.Domain.Entities;

namespace Erp.Application.Mappings;

public class ProductCategoryProfile : Profile
{
    public ProductCategoryProfile()
    {
        CreateMap<ProductCategoryCreateDto, ProductCategory>();
    }
}