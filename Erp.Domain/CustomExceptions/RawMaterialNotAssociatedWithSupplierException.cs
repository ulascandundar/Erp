using System.Net;
using Erp.Domain.Constants;

namespace Erp.Domain.CustomExceptions
{
    public class RawMaterialNotAssociatedWithSupplierException : NonLoggableException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

        public RawMaterialNotAssociatedWithSupplierException() : base(ResourceKeys.Errors.RawMaterialNotAssociatedWithSupplier)
        {
        }

        public RawMaterialNotAssociatedWithSupplierException(string message) : base(message)
        {
        }
    }
} 