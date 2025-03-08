using AutoMapper;
using Erp.Domain.DTOs.Customer;
using Erp.Domain.DTOs.User;
using Erp.Domain.Entities;
using Erp.Domain.Entities.NoSqlEntities;

namespace Erp.Application.Mappings;

public class UserProfile : Profile
{
	public UserProfile()
	{
		CreateMap<User, UserDto>();
		CreateMap<UserCreateDto, User>();
		CreateMap<UserUpdateDto, User>();
		CreateMap<Customer, CustomerDto>().ReverseMap();
	}
}
