using System.Net;

namespace ShopNetApi.Exceptions
{
    public class ConflictException : AppException
    {
        public ConflictException(string message)
            : base(message, (int)HttpStatusCode.Conflict)
        {
        }
    }
}
