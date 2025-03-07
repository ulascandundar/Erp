using Erp.Domain.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.DTOs.Order.Report;

public class OrderHistoryExcelDto
{
	[ExcelColumnName("Net Tutar")]
	public decimal NetAmount { get; set; }
	[ExcelColumnName("Toplam Tutar")]
	public decimal TotalAmount { get; set; }
	[ExcelColumnName("İndirim Tutarı")]
	public decimal DiscountAmount { get; set; }
	[ExcelColumnName("Toplam Ürün")]
	public decimal TotalQuantity { get; set; }
	[ExcelColumnName("Açıklama")]
	public string Description { get; set; }
	[ExcelColumnName("Tarih")]
	public string CreatedDate { get; set; }
	[ExcelColumnName("Saat")]
	public string CreatedTime { get; set; }
}
