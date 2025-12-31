using System.Net;

namespace ShopNetApi.Exceptions
{
    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message = "Unauthorized")
            : base(message, (int)HttpStatusCode.Unauthorized)
        {
        }
    }
}
