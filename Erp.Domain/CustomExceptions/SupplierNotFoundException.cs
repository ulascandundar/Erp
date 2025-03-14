using System.Net;
using Erp.Domain.Constants;

namespace Erp.Domain.CustomExceptions
{
    public class SupplierNotFoundException : NonLoggableException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;

        public SupplierNotFoundException() : base(ResourceKeys.Errors.SupplierNotFound)
        {
        }

        public SupplierNotFoundException(string message) : base(message)
        {
        }
    }
} 