using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Erp.Domain.Constants;

namespace Erp.Domain.CustomExceptions;

public class UserEmailAlreadyExistsException : NonLoggableException
{
	override public HttpStatusCode StatusCode => HttpStatusCode.OK;
	public UserEmailAlreadyExistsException() 
		: base(ResourceKeys.Errors.EmailAlreadyExists) { }
}
