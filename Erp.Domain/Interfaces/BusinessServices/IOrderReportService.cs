using Erp.Domain.DTOs.Order;
using Erp.Domain.DTOs.Order.Report;
using Erp.Domain.DTOs.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.Interfaces.BusinessServices;

public interface IOrderReportService
{
	Task<CumulativeOrderReport> GetCumulativeOrderReportAsync(CumulativeOrderReportRequestDto request);
	Task<CustomPagedResult<OrderHistoryDto>> GetPagedAsync(PaginationRequest request);
	Task<List<OrderHistoryExcelDto>> GetAllOrdersForExcel(PaginationRequest request);
	Task<CumulativeOrderProductGroupDto> GetCumulativeOrderWithProductGroupReportAsync(CumulativeOrderReportRequestDto request);
	Task<OrderDto> GetOrderDetail(Guid id);
}
