using AutoMapper;
using Erp.Domain.DTOs.RawMaterial;
using Erp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Application.Mappings;

public class RawMaterialProfile	: Profile
{
	public RawMaterialProfile()
	{
		CreateMap<RawMaterialCreateDto, RawMaterial>();
		CreateMap<RawMaterialUpdateDto, RawMaterial>();
		CreateMap<RawMaterial, RawMaterialDto>();
	}
}
