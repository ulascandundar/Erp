using Erp.Domain.DTOs.Pagination;
using Erp.Domain.DTOs.RawMaterial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.Interfaces;

public interface IRawMaterialService
{
	Task<RawMaterialDto> CreateRawMaterialAsync(RawMaterialCreateDto rawMaterialCreateDto);
	Task<RawMaterialDto> UpdateRawMaterialAsync(Guid id, RawMaterialUpdateDto rawMaterialUpdateDto);
	Task<RawMaterialDto> GetRawMaterialAsync(Guid id);
	Task<CustomPagedResult<RawMaterialDto>> GetRawMaterialsAsync(PaginationRequest paginationRequest);
	Task DeleteRawMaterialAsync(Guid id);
}
