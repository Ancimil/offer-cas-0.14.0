using System;

namespace Offer.Domain.Exceptions
{
    public class DuplicateObjectException : Exception
    {
        public DuplicateObjectException()
            : base()
        {

        }

        public DuplicateObjectException(string message)
            : base(message)
        {

        }

        public DuplicateObjectException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
