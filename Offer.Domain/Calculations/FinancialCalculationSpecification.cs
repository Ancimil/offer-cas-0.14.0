using System.Collections.Generic;

namespace Offer.Domain.Calculations
{
    public enum CalculationBasisType
    {
        AccountBalance = 0,
        TrancheBalance = 1,
        ObligationBalance = 2,
        TransactionAmount = 3,
        ContractAmount = 4,
        PrecalculatedAmount = 5
    }

    public enum AccountBalanceMeasureType
    {
        RunningBalance = 0,
        MaximalBalance = 1,
        MinimalBalance = 2,
        AverageBalance = 3,
        NetCreditTurnover = 4,
        NetDebitTurnover = 5,
        GrossCreditTurnover = 6,
        GrossDebitTurnover = 7,
        StartBalance = 8,
    }

    public class AccountBaseSpecification
    {
        private List<string> m_AccountingUnitTypes = new List<string>();

        public AccountBalanceMeasureType AccountBalanceMeasureType { get; set; }
        public List<string> AccountingUnitTypes
        {
            get { return m_AccountingUnitTypes; }
            set { m_AccountingUnitTypes = value; }
        }

    }

    public abstract class FinancialCalculationSpecification
    {
        private bool m_AllowNegativeAmount = true;

        public string Id { get; set; }
        public string TargetCurrency { get; set; }
        public int? Priority { get; set; }
        public int PriorityWithinDate { get; set; }
        public string BaseObligationType { get; set; }
        public string ResultingObligationType { get; set; }
        public string TaxModel { get; set; }
        public bool ResultingObligationByTranche { get; set; }
        public CalculationBasisType CalculationBasisType { get; set; }
        public AccountBaseSpecification BaseSpecification { get; set; }
        //public CurrencyConversionSpecificationCS CurrencyConversionSpecification { get; set; }
        public bool HasBalanceOnDebitSide { get; set; }
        public bool OrderByDebitFirstWithinSameDate { get; set; }
        public bool AllowSourceCurrencyDifference { get; set; }
        public bool AllowNegativeAmount
        {
            get { return m_AllowNegativeAmount; }
            set { m_AllowNegativeAmount = value; }
        }

        // public object AccountingScheme { get; set; }
        // public object CapitalizationAccountingScheme { get; set; }
    }

    public enum SimpleCashflowType
    {
        DisbursementFundedByFinancialInstitution = 0,
        DisbursementFundedWithSubsidy = 1,
        DisbursementFundedWithDownpayment = 2,
        CollateralExpense = 3,
        OtherExpense = 4,
        Other = 5,
        Deposit = 6,
        Withdrawal = 7
    }

    public class SimpleCashflowSpecification : FinancialCalculationSpecification
    {
        public SimpleCashflowType CashflowType { get; set; }
    }

    public class InterestCalculationSpecification : FinancialCalculationSpecification
    {
        public string InterestType { get; set; }
        public bool PerformInterestCorrecionAtTheEnd { get; set; }
        public bool OnlyCalculateInterestIfPositiveBalance { get; set; }
        public decimal? BaseCorrectionLimit { get; set; }
        public decimal? InterestCorrectionLimit { get; set; }
    }

    public class FeeCalculationSpecification : FinancialCalculationSpecification
    {
        public string FeeType { get; set; }

    }

    public enum RemainingDebtLevellingMethod
    {
        AdjustFirstAnnuityToFitPlannedDebt = 0,
        RecalculateAnnuityDueToActualDebt = 1,
        SuspendArrangementAndRequestManualReview = 2,
        ContinueAsNormally = 3,
    }

    public class AmortizationSpecification : FinancialCalculationSpecification
    {
        private List<FinancialCalculationSpecification> m_InvolvedCalcuations = new List<FinancialCalculationSpecification>();
        private List<string> m_InvolvedInterestTypes = new List<string>();
        private List<string> m_InvolvedFeeTypes = new List<string>();
        private bool m_IsPrincipalAmortization = true;

        public RemainingDebtLevellingMethod DebtLevellingMethod { get; set; }
        public List<FinancialCalculationSpecification> InvolvedCalcuations
        {
            get { return m_InvolvedCalcuations; }
            set { m_InvolvedCalcuations = value; }
        }
        public List<string> InvolvedInterestTypes
        {
            get { return m_InvolvedInterestTypes; }
            set { m_InvolvedInterestTypes = value; }
        }
        public List<string> InvolvedFeeTypes
        {
            get { return m_InvolvedFeeTypes; }
            set { m_InvolvedFeeTypes = value; }
        }
        public bool IsPrincipalAmortization
        {
            get { return m_IsPrincipalAmortization; }
            set { m_IsPrincipalAmortization = value; }
        }
    }
}
