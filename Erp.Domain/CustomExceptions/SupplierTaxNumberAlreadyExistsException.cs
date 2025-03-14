using System.Net;
using Erp.Domain.Constants;

namespace Erp.Domain.CustomExceptions
{
    public class SupplierTaxNumberAlreadyExistsException : NonLoggableException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.OK;

        public SupplierTaxNumberAlreadyExistsException() : base(ResourceKeys.Errors.SupplierTaxNumberAlreadyExists)
        {
        }

        public SupplierTaxNumberAlreadyExistsException(string message) : base(message)
        {
        }
    }
} 