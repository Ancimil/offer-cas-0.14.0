using System;

namespace Offer.Domain.Exceptions
{
    public class ApplicationNotFoundException : Exception
    {
        public ApplicationNotFoundException()
            : base()
        {

        }

        public ApplicationNotFoundException(string message)
            : base(message)
        {

        }

        public ApplicationNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
