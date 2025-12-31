using System.Net;

namespace ShopNetApi.Exceptions
{
    public class BadRequestException : AppException
    {
        public BadRequestException(string message) : base(message, (int)HttpStatusCode.BadRequest)
        {
        }
    }
}
