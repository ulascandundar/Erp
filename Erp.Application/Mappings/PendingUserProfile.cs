using AutoMapper;
using Erp.Domain.DTOs.PenginUser;
using Erp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Application.Mappings;

public class PendingUserProfile : Profile
{
	public PendingUserProfile()
	{
		CreateMap<PendingUserCreateDto, PendingUser>();
		CreateMap<PendingUser, PendingUserResponseDto>();
	}
}
