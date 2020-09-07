using System;

namespace Offer.Domain.Exceptions
{
    public class InvalidCalculationException : Exception
    {
        public InvalidCalculationException()
            : base()
        {

        }

        public InvalidCalculationException(string message)
            : base(message)
        {

        }

        public InvalidCalculationException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
