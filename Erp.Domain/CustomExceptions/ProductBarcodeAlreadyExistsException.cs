using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.CustomExceptions;

public class ProductBarcodeAlreadyExistsException : NonLoggableException
{
	override public HttpStatusCode StatusCode => HttpStatusCode.OK;
	public static string message = "Bu ürün barkodu zaten kayıtlı.";
	public ProductBarcodeAlreadyExistsException() : base(message)
	{
	}
}
