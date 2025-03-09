using AutoMapper;
using Erp.Domain.DTOs.ProductFormula;
using Erp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Application.Mappings;

public class ProductFormulaProfile : Profile
{
    public ProductFormulaProfile()
    {
        CreateMap<ProductFormula, ProductFormulaDto>();
        CreateMap<ProductFormulaItem, ProductFormulaItemDto>();
        
        CreateMap<ProductFormulaCreateDto, ProductFormula>();
        CreateMap<ProductFormulaItemCreateDto, ProductFormulaItem>();
        
        CreateMap<ProductFormulaUpdateDto, ProductFormula>();
        CreateMap<ProductFormulaItemUpdateDto, ProductFormulaItem>();
    }
} 