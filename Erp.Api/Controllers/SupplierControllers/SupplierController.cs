using Erp.Api.Controllers.Abstracts;
using Erp.Domain.DTOs.Pagination;
using Erp.Domain.DTOs.Supplier;
using Erp.Domain.Enums;
using Erp.Domain.Interfaces.BusinessServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Erp.Api.Controllers.SupplierControllers;

[Authorize(Roles = Roles.CompanyAdmin)]
public class SupplierController : BaseV1Controller
{
    private readonly ISupplierService _supplierService;
    private readonly ILocalizationService _localizationService;

    public SupplierController(ISupplierService supplierService, ILocalizationService localizationService)
    {
        _supplierService = supplierService;
        _localizationService = localizationService;
    }

    /// <summary>
    /// Creates a new supplier
    /// </summary>
    /// <param name="supplierCreateDto">Supplier data</param>
    /// <returns>Created supplier</returns>
    [HttpPost]
    public async Task<IActionResult> CreateSupplier([FromBody] SupplierCreateDto supplierCreateDto)
    {
        var result = await _supplierService.CreateSupplierAsync(supplierCreateDto);
        return CustomResponse(result);
    }

    /// <summary>
    /// Updates an existing supplier
    /// </summary>
    /// <param name="id">Supplier ID</param>
    /// <param name="supplierUpdateDto">Updated supplier data</param>
    /// <returns>Updated supplier</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSupplier(Guid id, [FromBody] SupplierUpdateDto supplierUpdateDto)
    {
        var result = await _supplierService.UpdateSupplierAsync(id, supplierUpdateDto);
        return CustomResponse(result);
    }

    /// <summary>
    /// Deletes a supplier
    /// </summary>
    /// <param name="id">Supplier ID</param>
    /// <returns>Success message</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSupplier(Guid id)
    {
        await _supplierService.DeleteSupplierAsync(id);
        return CustomResponse();
    }

    /// <summary>
    /// Gets a supplier by ID
    /// </summary>
    /// <param name="id">Supplier ID</param>
    /// <returns>Supplier</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSupplier(Guid id)
    {
        var result = await _supplierService.GetSupplierByIdAsync(id);
        return CustomResponse(result);
    }

    /// <summary>
    /// Gets a paged list of suppliers
    /// </summary>
    /// <param name="paginationRequest">Pagination parameters</param>
    /// <returns>Paged list of suppliers</returns>
    [HttpGet]
    public async Task<IActionResult> GetSuppliers([FromQuery] PaginationRequest paginationRequest)
    {
        var result = await _supplierService.GetSuppliersAsync(paginationRequest);
        return CustomResponse(result);
    }

} 