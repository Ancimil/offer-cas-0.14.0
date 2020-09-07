using CalculationService.Services;
using System;
using System.Collections.Generic;

namespace CalculationService.Models
{
    public class CalculateInstallmentPlanRequest
    {
        public string Term { get; set; }
        public double? InterestRate { get; set; }
        // public ArrangementRequest ArrangementRequest { get; set; }
        public string ArrangementType { get; set; }
        public string AccountNumber { get; set; }
        public string Currency { get; set; }
        public bool DealWithHisotry { get; set; } = false;
        public double? Amount { get; set; }
        public DateTime? StartDate { get; set; }
        public int InterestCalculationFrequencyPeriod { get; set; } = 1;
        public SimpleUnitOfTime InterestCalculationFrequencyUnitOfTime { get; set; } = SimpleUnitOfTime.M;
        public bool InterestAlwaysWithInstallment { get; set; } = true;
        public int CustomPrincipalFlowOption { get; set; } = 0;
        public DateTime? StartingBalanceDate { get; set; }
        public string InterestCalculationMethod { get; set; } = "9-0";
        public DateTime? FirstInstallmentDate { get; set; }
        public int InstallmentFrequencyPeriod { get; set; } = 1;
        public SimpleUnitOfTime InstallmentFrequencyUnitOfTime { get; set; } = SimpleUnitOfTime.M;
        public bool FollowTheEndOfMonth { get; set; } = false;
        public int RepaymentType { get; set; } = 1;
        public string ConditionContainer { get; set; } = "";
        public string TrancheNumber { get; set; } = "0";
        public double RegularInterestPercentage { get; set; }
        public SimpleUnitOfTime RegularInterestUnitOfTime { get; set; } = SimpleUnitOfTime.Y;
        public string CustomerIdentifier { get; set; } = "";
        public string Product { get; set; } = "0";
        public double FixedAnnuity { get; set; } = 0;
        public List<PricedScheduledPeriod> Periods { get; set; }

        // Origination fee
        public double? OriginationFeePercentage { get; set; }
        public double? OriginationFeeFixedAmount { get; set; }
        public double? OriginationFeeFixedAmountLcl { get; set; }
        public double OriginationFeeLowerLimit { get; set; } = 0;
        public double OriginationFeeLowerLimitLclValAmount { get; set; } = 0;
        public double OriginationFeeUpperLimit { get; set; } = 0;
        public double OriginationFeeUpperLimitLclValAmount { get; set; } = 0;
        public bool OriginationFeeCapitalization { get; set; } = false;

        // Management fee
        public bool ExcludeManagementFeeFromEAPR { get; set; } // TODO Check default
        public DateTime? FirstManagementFeeDate { get; set; }
        public bool IncludeManagementFeeInAnnuity { get; set; } // TODO Check default
        public int ManagementFeeCalculationFrequencyPeriod { get; set; } = 1;
        public SimpleUnitOfTime ManagementFeeCalculationFrequencyUnitOfTime { get; set; } = SimpleUnitOfTime.M;
        public double ManagementFeeLowerLimit { get; set; } = 0;
        public double ManagementFeePercentage { get; set; } = 0;
        // public SimpleUnitOfTime ManagementFeeUnitOfTime { get; set; } // mislim da ne treba

        // Graceperiod
        public DateTime? FirstInterestDateInGrace { get; set; }
        public DateTime? GracePeriodEnd { get; set; }
        public bool FirstInterestDateInGraceFollowTheEndOfMonth { get; set; } = false;
        public int InterestCalculationFrequencyInGracePeriod { get; set; } = 1;
        public SimpleUnitOfTime InterestCalculationFrequencyInGraceUnitOfTime { get; set; } = SimpleUnitOfTime.M;

        // Drawdown period
        public DateTime? FirstInterestDateInDrawdownPeriod { get; set; }
        public DateTime? DrawdownPeriodEnd { get; set; }

        // Repayment period
        public DateTime? FirstInterestDateInRepaymentPeriod { get; set; }
        public bool FirstInterestDateInRepaymentFollowTheEndOfMonth { get; set; } = false;

    }
}
