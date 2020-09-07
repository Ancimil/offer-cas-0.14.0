using System;

namespace PriceCalculation.Exceptions
{
    public class MaxNumberOfIterationsException : Exception
    {
        public MaxNumberOfIterationsException()
            : base()
        {

        }

        public MaxNumberOfIterationsException(string message)
            : base(message)
        {

        }

        public MaxNumberOfIterationsException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
