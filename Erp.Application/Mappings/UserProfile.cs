using AutoMapper;
using Erp.Domain.DTOs.User;
using Erp.Domain.Entities;

namespace Erp.Application.Mappings;

public class UserProfile : Profile
{
	public UserProfile()
	{
		CreateMap<User, UserDto>();
		CreateMap<UserCreateDto, User>();
		CreateMap<UserUpdateDto, User>();
	}
}
