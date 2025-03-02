using Erp.Api.Controllers.Abstracts;
using Erp.Domain.DTOs.Company;
using Erp.Domain.DTOs.Pagination;
using Erp.Domain.Enums;
using Erp.Domain.Interfaces.BusinessServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.AdminControllers;

[Authorize(Roles = Roles.Admin)]
public class CompanyController : BaseV1Controller
{
    private readonly ICompanyService _companyService;

    public CompanyController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _companyService.GetAllCompaniesAsync();
        return CustomResponse(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _companyService.GetCompanyByIdAsync(id);
        return CustomResponse(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _companyService.SoftDeleteCompanyAsync(id);
        return CustomResponse();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CompanyCreateDto companyCreateDto)
    {
        var result = await _companyService.CreateCompanyAsync(companyCreateDto);
        return CustomResponse(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromBody] CompanyUpdateDto companyUpdateDto, Guid id)
    {
        var result = await _companyService.UpdateCompanyAsync(companyUpdateDto, id);
        return CustomResponse(result);
    }

    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged([FromQuery] PaginationRequest paginationRequest)
    {
        var result = await _companyService.GetPagedAsync(paginationRequest);
        return CustomResponse(result);
    }
} 