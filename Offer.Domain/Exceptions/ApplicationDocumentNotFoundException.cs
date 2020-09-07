using System;

namespace Offer.Domain.Exceptions
{
    public class ApplicationDocumentNotFoundException : Exception
    {
        public ApplicationDocumentNotFoundException()
            : base()
        {

        }

        public ApplicationDocumentNotFoundException(string message)
            : base(message)
        {

        }

        public ApplicationDocumentNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
