using MediatR;

namespace Offer.API.Application.Commands
{
    public class SetSuppliersBuyersReportCommand : IRequest<bool?>
    {
        public long ApplicationNumber { get; set; }
        public string CustomerNumber { get; set; }
        public long? SuppliersBuyersReportId { get; set; }

        public SetSuppliersBuyersReportCommand(long applicationNumber, string customerNumber, long? suppliersBuyersReportId)
        {
            ApplicationNumber = applicationNumber;
            CustomerNumber = customerNumber;
            SuppliersBuyersReportId = suppliersBuyersReportId;
        }
    }
}
