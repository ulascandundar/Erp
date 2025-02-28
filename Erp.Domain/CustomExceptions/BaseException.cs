using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.CustomExceptions;

public abstract class BaseException : Exception
{
	public virtual HttpStatusCode StatusCode => HttpStatusCode.InternalServerError; // Default 500

	protected BaseException(string message) : base(message) { }
}
