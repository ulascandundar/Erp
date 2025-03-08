using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.CustomExceptions;

public class CompanyTaxNumberAlreadyExistException : NonLoggableException
{
	override public HttpStatusCode StatusCode => HttpStatusCode.OK;
	public static string message = "Bu vergi numarası zaten kayıtlı.";
	public CompanyTaxNumberAlreadyExistException() : base(message)
	{
	}
}
