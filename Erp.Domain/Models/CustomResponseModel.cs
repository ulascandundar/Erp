using Erp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.Models;

public class CustomResponseModel
{
	public bool IsSuccess { get; set; }
	public string Message { get; set; }
	public object Data { get; set; }
	public CustomFrontendActions? CustomFrontendAction { get; set; }
}
