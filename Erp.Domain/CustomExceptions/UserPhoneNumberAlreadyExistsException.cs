using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.CustomExceptions;

public class UserPhoneNumberAlreadyExistsException : NonLoggableException
{
	override public HttpStatusCode StatusCode => HttpStatusCode.OK; 
	public static string message = "Bu telefon numarası zaten kayıtlı.";
	public UserPhoneNumberAlreadyExistsException() : base(message)
	{
	}
}
