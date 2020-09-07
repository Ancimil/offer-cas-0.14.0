namespace Offer.Domain.AggregatesModel.ContentModel
{
    public class ErrorResponse
    {
        public string Problem { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
    }
}
