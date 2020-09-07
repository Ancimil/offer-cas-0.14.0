namespace CalculationService.Models
{
    public abstract class CalculationRequest
    {
        public string Url { get; set; }
        public string RouteIdentifier { get; set; }
        public string ArrangementType { get; set; }
        public string ClientIdentifier { get; set; }
    }
}
