using AutoMapper;
using Erp.Domain.DTOs.Company;
using Erp.Domain.Entities;

namespace Erp.Application.Mappings;

public class CompanyProfile : Profile
{
    public CompanyProfile()
    {
        CreateMap<Company, CompanyDto>();
        CreateMap<CompanyCreateDto, Company>();
        CreateMap<CompanyUpdateDto, Company>();
    }
} 