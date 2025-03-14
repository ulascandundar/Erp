using System.Net;
using Erp.Domain.Constants;

namespace Erp.Domain.CustomExceptions
{
    public class RawMaterialAlreadyAssociatedWithSupplierException : NonLoggableException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

        public RawMaterialAlreadyAssociatedWithSupplierException() : base(ResourceKeys.Errors.RawMaterialAlreadyAssociatedWithSupplier)
        {
        }

        public RawMaterialAlreadyAssociatedWithSupplierException(string message) : base(message)
        {
        }
    }
} 