using AutoMapper;
using Erp.Domain.DTOs.RawMaterialSupplier;
using Erp.Domain.DTOs.Supplier;
using Erp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Application.Mappings;

public class SupplierProfile : Profile
{
    public SupplierProfile()
    {
        CreateMap<Supplier, SupplierDto>();
        CreateMap<SupplierCreateDto, Supplier>();
        CreateMap<SupplierUpdateDto, Supplier>();
        
        CreateMap<RawMaterialSupplier, RawMaterialSupplierDto>();
        CreateMap<RawMaterialSupplierCreateDto, RawMaterialSupplier>();
    }
} 