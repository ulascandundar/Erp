using System.Net;
using Erp.Domain.Constants;

namespace Erp.Domain.CustomExceptions
{
    public class SupplierHasRawMaterialsException : NonLoggableException
    {
        public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

        public SupplierHasRawMaterialsException() : base(ResourceKeys.Errors.SupplierHasRawMaterials)
        {
        }

        public SupplierHasRawMaterialsException(string message) : base(message)
        {
        }
    }
}