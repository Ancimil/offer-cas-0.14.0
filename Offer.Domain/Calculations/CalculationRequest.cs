using MicroserviceCommon.Models;
using PriceCalculation.Models.Lifecycle;
using System;
using System.Collections.Generic;

namespace Offer.Domain.Calculations
{
    public enum NonWorkingDayResolutionType
    {
        FirstWorkingDayAfter = 0,
        FirstWorkingDayBefore = 1,
        TreatAllDaysAsWorking = 2,
        FirstWorkingDayAfterExceptLastOneThatGoesBefore = 3,
        FirstWorkingDayAfterExceptLastOne = 4,
        FirstWorkingDayBeforeExceptLastOne = 5,
        FirstWorkingDayAfterIfDoNotMoveInNextMonth = 6
    }
    public enum OccurrenceStatus
    {
        Undue = 0,
        Due = 1,
        Completed = 2
    }
    public class CalculationOccurrence
    {

        /* 
        // mozda ce ovaj ovverride trebati...
        public override bool Equals(object obj) // morao sam da uradim override da gleda na pocetku ReferenceEquals() jer se ova metoda poziva kod List.Remove()
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return base.Equals(obj);
        }
        */

        public string ActivitySpecificationCode { get; set; }
        public FinancialCalculationSpecification ActivitySpecification { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime? NextCalculationDate { get; set; }
        public OccurrenceStatus Status { get; set; }
        public string Occurrence { get; set; } // hmm... sta li ovo bese, bio je reference
        public string FinancialObligation { get; set; } // hmm... sta li ovo bese, bio je reference
        public string Currency { get; set; }
        public Decimal? PlannedBaseAmount { get; set; }
        public Decimal? PlannedAmount { get; set; }
        public Decimal? PlannedOutsandingAmount { get; set; }
        public Boolean IsCustom { get; set; }
        public string Tranche { get; set; }
        public string ReferentExchangeRateTypeForLocalCurrency { get; set; }
        public bool IsLastAmortization { get; set; }
        public bool? RecalculateAnnuity { get; set; }
        public CalculationTrigger Trigger { get; set; } // parent trigger
        public virtual OccurenceType Type // Ovo ne treba još uvek, to će trebati eventualno za anekse
        {
            get { return OccurenceType.RegularScheduled; }
        }

    }

    public enum OccurenceType // mora ovim redosledom zbog poredjenja kod sortiranja!!! 
    {
        RegularScheduled = 0,

        ResetCalculation = 1,

        PreviousCalculation = 2,
    }

    public enum ScheduleRecurrenceType
    {
        SingleOccurrence = 0,
        RecurrenceWithLimitedPeriod = 1,
        RecurrenceWithLimitedOccurrences = 2,
        RecurrenceWithoutEndSpecified = 3,
        Custom = 4
    }

    public class EffectivePeriod
    {
        private DateTime m_StartDate;
        private DateTime m_EndDate;

        public EffectivePeriod()
        {
            m_StartDate = DateTime.Now.Date;
            m_EndDate = DateTime.Now.Date;
        }

        public EffectivePeriod(DateTime startDate, DateTime endDate)
        {
            m_StartDate = startDate;
            m_EndDate = endDate;
        }

        public DateTime StartDate
        {
            get { return m_StartDate; }
            set { m_StartDate = value; }
        }
        public DateTime EndDate
        {
            get { return m_EndDate; }
            set { m_EndDate = value; }
        }
        public override string ToString()
        {
            return string.Format("{0} - {1}", m_StartDate.ToShortDateString(), m_EndDate.ToShortDateString());
        }
    }

    public enum RecurrencePattern
    {
        Daily = 0,
        Weekly = 1,
        Monthly = 2,
        Yearly = 3
    }

    public enum DailyRecurrencePatternType
    {
        EveryNDays = 0,
        EveryWeekday = 1
    }

    public enum MonthlyRecurrencePatternType
    {
        DayOfMonth = 0,
        DayOfWeekOfMonth = 1
    }

    public enum WeekOfMonth
    {
        First = 0,
        Second = 1,
        Third = 2,
        Fourth = 3,
        Last = 4
    }
    public enum MonthOfYear
    {
        January = 0,
        February = 1,
        March = 2,
        April = 3,
        May = 4,
        Jun = 5,
        July = 6,
        August = 7,
        September = 8,
        October = 9,
        November = 10,
        December = 11
    }

    public class CalculationRecurrence
    {
        public Int32 IterationPeriodNumerator { get; set; }
        public EffectivePeriod Range { get; set; }
        public Int32 MaxOccurances { get; set; }
        public RecurrencePattern RecurrencePattern { get; set; }
        public DailyRecurrencePatternType? DailyRecurrencePatternType { get; set; }
        public MonthlyRecurrencePatternType? MonthlyRecurrencePatternType { get; set; }
        public Int32? DayOfMonth { get; set; }
        public DayOfWeek? DayOfWeek { get; set; }
        public WeekOfMonth? WeekOfMonth { get; set; }
        public MonthOfYear? MonthOfYear { get; set; }
        public DateTime OccurrencesCalculatedUpTo { get; set; }
        public Boolean? FollowTheEndOfMonth { get; set; }
    }
    public enum SimpleUnitOfTime
    {
        D = 0,
        M = 1,
        Y = 2
    }
    public class TimePeriod
    {
        public int Period { get; set; }
        public SimpleUnitOfTime UnitOfTime { get; set; }
        public override string ToString()
        {
            return string.Format("{0}{1}", Period, UnitOfTime);
        }
    }
    public class ReferentRateOffsetRule
    {
        public Int32? DayOfMonth { get; set; }
        public TimePeriod PeriodOffset { get; set; }
    }

    public class CalculationTrigger
    {
        private readonly List<CalculationOccurrence> m_Occurrences = new List<CalculationOccurrence>();

        private readonly ScheduleRecurrenceType m_ScheduleRecurrenceType = ScheduleRecurrenceType.SingleOccurrence;

        public string ActivitySpecificationCode { get; set; }
        public FinancialCalculationSpecification ActivitySpecification { get; set; }
        public DateTime TriggerStartDate { get; set; }
        public DateTime? TriggerEndDate { get; set; }
        public Currency TriggerAmount { get; set; }
        public Decimal? RepaymentPercentage { get; set; }
        public string ReferentExchangeRateTypeForLocalCurrency { get; set; }
        public Boolean? ExecuteAtTheBeginningOfThePeriod { get; set; }
        public Boolean? ExecuteAtTheEndOfThePeriod { get; set; }
        public string NonWorkingDayCalendarCode { get; set; }
        public NonWorkingDayCalendar NonWorkingDayCalendar { get; set; }
        public NonWorkingDayResolutionType? NonWorkingDayRule { get; set; }
        public List<CalculationOccurrence> Occurrences { get; set; }
        public CalculationRecurrence Recurrence { get; set; }
        public ScheduleRecurrenceType ScheduleRecurrenceType { get; set; }
        public ReferentRateOffsetRule ReferentRateOffset { get; set; }
        public CalculationPlan CalculationPlan { get; set; }

        #region Calculated fields 
        decimal? Amout
        {
            get
            {
                if (this.TriggerAmount != null)
                {
                    return this.TriggerAmount.Amount;
                }
                else
                {
                    return null;
                }
            }
        }

        string Currency
        {
            get
            {
                if (this.TriggerAmount != null)
                {
                    return this.TriggerAmount.Code;
                }
                else
                {
                    return null;
                }
            }
        }

        DateTime? PlanStartDate
        {
            get
            {
                if (this.CalculationPlan != null)
                {
                    return this.CalculationPlan.PlanStartDate;
                }
                else
                {
                    return null;
                }
            }
        }

        DateTime? PlanEndDate
        {
            get
            {
                if (this.CalculationPlan != null)
                {
                    return this.CalculationPlan.PlanEndDate;
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion

    }

    public class CalculationPlan
    {
        private List<CalculationTrigger> m_Triggers = new List<CalculationTrigger>();

        public DateTime PlanStartDate { get; set; }
        public DateTime? PlanEndDate { get; set; }
        public Decimal? RemainingDebtAtTheEnd { get; set; }
        public List<string> DebtAccountingUnitTypes { get; set; }
        public decimal? PlannedAnnuity { get; set; }
        public string Tranche { get; set; }
        public List<CalculationTrigger> Triggers
        {
            get { return m_Triggers; }
            set { m_Triggers = value; }
        }

        public Decimal? RemainingDebtPercentage { get; set; }
        
        public bool IsLiability { get; set; }

    }
       
    public enum ExcludeCalculationsFromAnnuity
    {
        All = 0,
        OnlyFees = 1
    }


    public abstract class ConditionInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime EffectiveDate { get; set; }

        public abstract ConditionInfo Clone();
    }

    public enum InterestRateTierCalculationType
    {
        Scaled = 0,
        Interval = 1,
        //Mixed = 2,
    }

    public class InterestRateInfo : ConditionInfo
    {
        private List<TermTier> m_TermTiers = new List<TermTier>();
        private List<BalanceTier> m_BalanceTiers = new List<BalanceTier>();

        public string Name { get; set; }
        public string InterestType { get; set; } // mi imamo enumeraciju, ali ovde moze da ostane string... ionako nije bitno
        public string Currency { get; set; }
        public decimal RatePercentage;
        public SimpleUnitOfTime RateUnitOfTime { get; set; }
        public CalendarBasisKind CalendarBasis { get; set; }
        public bool IsCompound { get; set; }
        public InterestRateTierCalculationType TierCalculationType { get; set; }
        public List<BalanceTier> BalanceTiers
        {
            get { return m_BalanceTiers; }
            set { m_BalanceTiers = value; }
        }
        public List<TermTier> TermTiers
        {
            get { return m_TermTiers; }
            set { m_TermTiers = value; }
        }
        public bool? RecalculateAnnuity { get; set; }

        public override ConditionInfo Clone()
        {
            InterestRateInfo ci = new InterestRateInfo
            {
                BalanceTiers = this.BalanceTiers,
                Currency = this.Currency,
                EffectiveDate = this.EffectiveDate,
                CalendarBasis = this.CalendarBasis,
                IsCompound = this.IsCompound,
                InterestType = this.InterestType,
                Id = this.Id,
                Name = this.Name,
                RatePercentage = this.RatePercentage,
                RateUnitOfTime = this.RateUnitOfTime,
                TermTiers = this.TermTiers,
                TierCalculationType = this.TierCalculationType,
                RecalculateAnnuity = this.RecalculateAnnuity
            };
            return ci;
        }
    }

    public abstract class InterestRateTier
    {
        private readonly string  m_Id;

        public InterestRateTier(string tierIdentifer)
        {
            if (string.IsNullOrEmpty(tierIdentifer))
            {
                throw new ArgumentException("Identifer for tier must be set. It can not be null or empty string.");
            }

            m_Id = tierIdentifer;
        }
        public bool? LowerInclusive { get; set; }
        public bool? UpperInclusive { get; set; }
        public decimal RateCorrectionPercentage { get; set; }
        public decimal? BalanceReductionPercentage { get; set; }
        public bool IsDefaultTier { get; set; }
        public bool SkipInterestCalculation { get; set; }
    }

    public class BalanceTier : InterestRateTier
    {
        public BalanceTier(string tierIdentifer)
            : base(tierIdentifer)
        {

        }

        public decimal? LowerLimitFixedAmount { get; set; }
        public string LowerLimitType { get; set; }
        public decimal? UpperLimitFixedAmount { get; set; }
        public string UpperLimitType { get; set; }
    }

    public class TermTier : InterestRateTier
    {
        public TermTier(string tierIdentifer)
            : base(tierIdentifer)
        {

        }

        public TimePeriod LowerPeriod { get; set; }
        public TimePeriod UpperPeriod { get; set; }
    }

    public class FeeInfo : ConditionInfo
    {
        public string FeeType { get; set; }
        public Currency FixedAmount { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? MinimalAmount { get; set; }
        public decimal? MaximalAmount { get; set; }
        public string LimitCurrency { get; set; }
        public override ConditionInfo Clone()
        {
            FeeInfo ci = new FeeInfo
            {
                EffectiveDate = this.EffectiveDate,
                FeeType = this.FeeType,
                FixedAmount = this.FixedAmount,
                Id = this.Id,
                Name = this.Name,
                Percentage = this.Percentage,
                MinimalAmount = this.MinimalAmount,
                MaximalAmount = this.MaximalAmount,
                LimitCurrency = this.LimitCurrency
            };
            return ci;
        }
    }


    /*
    public abstract class CalculationRequestBase
    {
        public DateTime? CalculationStart { get; set; }

        public DateTime? CalculationEnd { get; set; }
    }

    public class CalculationRequest : CalculationRequestBase
    {
        public string ArrangementNumber { get; set; } // Arrangement
        public string ArrangementKind { get; set; } // ArrangementType
        public String Currency { get; set; }
        public string CustomerNumber { get; set; } // Customer
        public string ProductCode { get; set; } // Product
        public string OrganizationUnit { get; set; }
        public string ResponsibleAgent { get; set; }
    }
    */

    public class CalculationRequest
    {
        #region Fields

        private List<CalculationPlan> m_Plans = new List<CalculationPlan>();
        private List<CalculationOccurrence> m_Occurrences = new List<CalculationOccurrence>();
        public List<InterestRateInfo> m_InterestRates = new List<InterestRateInfo>();
        public List<FeeInfo> m_Fees = new List<FeeInfo>();

        /*
        private Reference<InterestRateContainer> m_InterestRateContainer;
        private Reference<FeeRateContainer> m_FeeRateContainer;
        private Reference<LimitConditionContainer> m_LimitConditionContainer;
        */

        #endregion

        #region Properties
        public DateTime? CalculationStart { get; set; }
        public DateTime? CalculationEnd { get; set; }

        public List<CalculationOccurrence> Occurrences
        {
            get { return m_Occurrences; }
            set { m_Occurrences = value; }
        }
        public List<CalculationPlan> Plans
        {
            get { return m_Plans; }
            set { m_Plans = value; }
        }
        public List<InterestRateInfo> InterestRates
        {
            get { return m_InterestRates; }
            set { m_InterestRates = value; }
        }
        public List<FeeInfo> Fees
        {
            get { return m_Fees; }
            set { m_Fees = value; }
        }

        // ovo je konfiguracija prikaza amortplana, za sad se ignorise, ali trebace
        public string InstallmentPlanViewIdentifier { get; set; }
        public int? DecimalsToRoundCalculatedAmounts { get; set; }
        public ExcludeCalculationsFromAnnuity? ExcludeCalculationsFromAnnuity { get; set; }
        #endregion
    }
}
