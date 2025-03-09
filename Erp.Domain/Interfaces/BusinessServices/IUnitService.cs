using Erp.Domain.DTOs.Pagination;
using Erp.Domain.DTOs.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.Interfaces.BusinessServices;

public interface IUnitService
{
	Task<UnitDto> CreateUnitAsync(UnitCreateDto unitCreateDto);
	Task<UnitDto> UpdateUnitAsync(Guid id, UnitUpdateDto unitUpdateDto);
	Task DeleteAsync(Guid id);
	Task<UnitDto> GetByIdAsync(Guid id);
	Task<CustomPagedResult<UnitDto>> GetPagedAsync(PaginationRequest paginationRequest);
	Task<List<UnitDto>> GetRelationUnits(Guid unitId);
	Task<decimal> FindRateToRootAsync(Guid unitId);
	Task<decimal> ConvertUnit(Guid unitId, Guid rawMaterialId);
}
