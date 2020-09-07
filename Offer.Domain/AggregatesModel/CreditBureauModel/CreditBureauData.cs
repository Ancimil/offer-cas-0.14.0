using System;
using System.Collections.Generic;

namespace Offer.Domain.AggregatesModel.CreditBureauModel
{
    public class CreditBureauData
    {
        public Decimal ActiveGuaranteeAmount { get; set; }
        public Decimal ActiveGuaranteeLeasingAmount { get; set; }
        public decimal? ActualLatencyAmount { get; set; }
        public Int32? ActualLatencyNumberOfDays { get; set; }
        public List<Placement> Placements { get; set; }
        public List<OffBalance> OffBalances { get; set; }
        public Decimal? CreditIndeptness { get; set; }
        public decimal? CurrentExposure { get; set; }
        public Decimal? FXRisk { get; set; }
        public Boolean? HadLatency { get; set; }
        public Boolean? HasHousingLoan { get; set; }
        public Int32? HistoricalArrearNumberOfDays { get; set; }
        public Boolean? IsCreditWorthy { get; set; }
        public Decimal? BadDebtLoanAmount { get; set; }
        public Decimal? BadDebtCreditCardAmount { get; set; }
        public Decimal? BadDebtAccountAmount { get; set; }
        public decimal? HistoricalArrearAmount { get; set; }
        public Decimal MonthlyObligations { get; set; }
        public Int32? NumberOfActiveCreditCards { get; set; }
        public Int32? NumberOfActiveLoans { get; set; }
        public Int32? NumberOfGuaranteeStastusDO { get; set; }
        public Int32? NumberOfActiveTermLoanDeptor { get; set; }
        public Int32? NumberOfActiveInsurance { get; set; }
        public Int32? TotalNumberOfActiveInsurance { get; set; }
        public Decimal? OffBalance { get; set; }
        public decimal? PotentialExposure { get; set; }
        public decimal? Salary { get; set; }
        public Int32? TotalNumberOfCreditCards { get; set; }
        public Int32? TotalNumberOfLoans { get; set; }
        public Decimal SumOfCreditCardObligations { get; set; }
        public Decimal SumOfGuarantorObligations { get; set; }
        public Decimal SumOfLoanObligations { get; set; }
        public Decimal? SumOfLeasingObligations { get; set; }
        public Decimal SumOfActivatedGuarantorObligations { get; set; }
        public string WorkingCurrency { get; set; }
        public DateTime RequestedAt { get; set; }
        public Boolean? HasCHFIndexedLoan { get; set; }
        public Int32? TotalNumberOfApplications { get; set; }
        public Int32? NumberOfApplications30Days { get; set; }
    }
}
