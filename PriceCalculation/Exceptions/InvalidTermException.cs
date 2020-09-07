using System;

namespace PriceCalculation.Exceptions
{
    public class InvalidTermException : Exception
    {
        public InvalidTermException()
            : base()
        {

        }

        public InvalidTermException(string message)
            : base(message)
        {

        }

        public InvalidTermException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
