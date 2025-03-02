using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.CustomExceptions;

public class ProductNameAlreadyExistsException : NonLoggableException
{
	override public HttpStatusCode StatusCode => HttpStatusCode.Conflict;
	public static string message = "Bu ürün adı zaten kayıtlı.";
	public ProductNameAlreadyExistsException() : base(message)
	{
	}
}
