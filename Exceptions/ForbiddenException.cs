using System.Net;

namespace ShopNetApi.Exceptions
{
    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message = "Forbidden")
            : base(message, (int)HttpStatusCode.Forbidden)
        {
        }
    }
}
