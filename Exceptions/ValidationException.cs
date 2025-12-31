using System.Net;

namespace ShopNetApi.Exceptions
{
    public class ValidationException : AppException
    {
        public object Errors { get; }

        public ValidationException(object errors)
            : base("Validation failed", (int)HttpStatusCode.UnprocessableEntity)
        {
            Errors = errors;
        }
    }
}
