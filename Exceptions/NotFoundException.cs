using System.Net;

namespace ShopNetApi.Exceptions
{
    public class NotFoundException : AppException
    {
        public NotFoundException(string message) : base(message, (int)HttpStatusCode.NotFound)
        {
        }
    }
}
