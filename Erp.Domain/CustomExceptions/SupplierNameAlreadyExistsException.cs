using System.Net;
using Erp.Domain.Constants;

namespace Erp.Domain.CustomExceptions
{
    public class SupplierNameAlreadyExistsException : NonLoggableException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.OK;

        public SupplierNameAlreadyExistsException() : base(ResourceKeys.Errors.SupplierNameAlreadyExists)
        {
        }

        public SupplierNameAlreadyExistsException(string message) : base(message)
        {
        }
    }
} 