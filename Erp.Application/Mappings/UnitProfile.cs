using AutoMapper;
using Erp.Domain.DTOs.Unit;
using Erp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Application.Mappings;

public class UnitProfile : Profile
{
	public UnitProfile()
	{
		CreateMap<Unit, UnitDto>();
		CreateMap<UnitCreateDto, Unit>();
		CreateMap<UnitUpdateDto, Unit>();
	}
}
