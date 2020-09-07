using System;

namespace Offer.Domain.Exceptions
{
    public class AnonymizeActiveOfferException : Exception
    {
        public AnonymizeActiveOfferException()
            : base()
        {

        }

        public AnonymizeActiveOfferException(string message)
            : base(message)
        {

        }

        public AnonymizeActiveOfferException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
