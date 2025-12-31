using System.Net;

namespace ShopNetApi.Exceptions
{
    public class InternalServerException : AppException
    {
        public InternalServerException(string message = "Internal server error")
            : base(message, (int)HttpStatusCode.InternalServerError)
        {
        }
    }
}
