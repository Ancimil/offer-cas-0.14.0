using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Text;

namespace CalculationService.Models
{
    public partial class InstallmentPlanRow : IEquatable<InstallmentPlanRow>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InstallmentPlanRow" /> class.
        /// </summary>
        /// <param name="Ordinal">Ordinal number of the row in plan.</param>
        /// <param name="Date">Payment due date.</param>
        /// <param name="ActivityKind">Enumeration which can be used by software to recognize the meaning of the activity that triggers the payment. For example, in combination with product type it can be used at front end as a resource id for row description translation into selected culture. Grouping multiple activities into single row is allowed, and then most appropriate classification should be used. For example, disbursement activity may include downpayment, fee-payment, and cash-collateral-pledging. Also repayment activity may include interest-payment, fee-payment, and cash-collateral-release. For deposit products, disbursement activity-kind is used to describe depositing activity, while repayment is used to describe withdrawal.\nFor a list of possible values see [installment-activity-kinds](arrangement-classifications.html#installment-activity-kinds) enumeration.\n.</param>
        /// <param name="Description">Description of the activity that triggers the payment. Should be translated in selected culture and front-end applications should use it as is. If description is not provided, front-end applications should use activity-kind to get appropriate description in selected culture..</param>
        /// <param name="Disbursement">Loan disbursement, deposit, or guarantee opening - principal balance increase.</param>
        /// <param name="StartingBalance">Principal balance before payment.</param>
        /// <param name="PrincipalRepayment">Principal amount to be repayed..</param>
        /// <param name="InterestRepayment">Interest amount to be payed.</param>
        /// <param name="Annuity">Sum of principal and interest to be repayed.</param>
        /// <param name="OutstandingBalance">Principal balance after payment.</param>
        /// <param name="Fee">Fee payment.</param>
        /// <param name="OtherExpenses">Other expenses.</param>
        /// <param name="CashCollateral">Positive and negative flows of cash collateral.</param>
        /// <param name="DeferredPayment">Part of payment that is being delayed for some future date. If such defferal exists, then the same amount must be deducted from principal repayment..</param>
        /// <param name="DeferredPaymentInstallment">Repayment of aggregated deferred payments.</param>
        /// <param name="NetCashFlow">Total net cash flow between the bank and the customer.</param>
        /// <param name="DiscountedNetCashFlow">Net cash flow that is being discounted to present value using calculated EAPR.</param>
        /// <param name="DiscountedDisbursement">Disbursement flows discounted using APR counted based on discounted-net-flow.</param>
        /// <param name="DiscountedCashCollateralFlow">Cash collateral flows discounted using APR counted based on discounted-net-flow.</param>
        public InstallmentPlanRow(int? Ordinal = null, DateTime? Date = null, string ActivityKind = null, string Description = null, double? Disbursement = null, double? StartingBalance = null, double? PrincipalRepayment = null, double? InterestRepayment = null, double? Annuity = null, double? OutstandingBalance = null, double? Fee = null, double? OtherExpenses = null, double? CashCollateral = null, double? DeferredPayment = null, double? DeferredPaymentInstallment = null, double? NetCashFlow = null, double? DiscountedNetCashFlow = null, double? DiscountedDisbursement = null, double? DiscountedCashCollateralFlow = null)
        {
            this.Ordinal = Ordinal;
            this.Date = Date;
            this.ActivityKind = ActivityKind;
            this.Description = Description;
            this.Disbursement = Disbursement;
            this.StartingBalance = StartingBalance;
            this.PrincipalRepayment = PrincipalRepayment;
            this.InterestRepayment = InterestRepayment;
            this.Annuity = Annuity;
            this.OutstandingBalance = OutstandingBalance;
            this.Fee = Fee;
            this.OtherExpenses = OtherExpenses;
            this.CashCollateral = CashCollateral;
            this.DeferredPayment = DeferredPayment;
            this.DeferredPaymentInstallment = DeferredPaymentInstallment;
            this.NetCashFlow = NetCashFlow;
            this.DiscountedNetCashFlow = DiscountedNetCashFlow;
            this.DiscountedDisbursement = DiscountedDisbursement;
            this.DiscountedCashCollateralFlow = DiscountedCashCollateralFlow;

        }


        /// <summary>
        /// Ordinal number of the row in plan
        /// </summary>
        /// <value>Ordinal number of the row in plan</value>
        [JsonProperty(PropertyName = "ordinal")]
        public int? Ordinal { get; set; }


        /// <summary>
        /// Payment due date
        /// </summary>
        /// <value>Payment due date</value>
        [JsonProperty(PropertyName = "date")]
        public DateTime? Date { get; set; }


        /// <summary>
        /// Enumeration which can be used by software to recognize the meaning of the activity that triggers the payment. For example, in combination with product type it can be used at front end as a resource id for row description translation into selected culture. Grouping multiple activities into single row is allowed, and then most appropriate classification should be used. For example, disbursement activity may include downpayment, fee-payment, and cash-collateral-pledging. Also repayment activity may include interest-payment, fee-payment, and cash-collateral-release. For deposit products, disbursement activity-kind is used to describe depositing activity, while repayment is used to describe withdrawal.\nFor a list of possible values see [installment-activity-kinds](arrangement-classifications.html#installment-activity-kinds) enumeration.\n
        /// </summary>
        /// <value>Enumeration which can be used by software to recognize the meaning of the activity that triggers the payment. For example, in combination with product type it can be used at front end as a resource id for row description translation into selected culture. Grouping multiple activities into single row is allowed, and then most appropriate classification should be used. For example, disbursement activity may include downpayment, fee-payment, and cash-collateral-pledging. Also repayment activity may include interest-payment, fee-payment, and cash-collateral-release. For deposit products, disbursement activity-kind is used to describe depositing activity, while repayment is used to describe withdrawal.\nFor a list of possible values see [installment-activity-kinds](arrangement-classifications.html#installment-activity-kinds) enumeration.\n</value>
        [JsonProperty(PropertyName = "activity-kind")]
        [DefaultValue("")]
        public string ActivityKind { get; set; }


        /// <summary>
        /// Description of the activity that triggers the payment. Should be translated in selected culture and front-end applications should use it as is. If description is not provided, front-end applications should use activity-kind to get appropriate description in selected culture.
        /// </summary>
        /// <value>Description of the activity that triggers the payment. Should be translated in selected culture and front-end applications should use it as is. If description is not provided, front-end applications should use activity-kind to get appropriate description in selected culture.</value>
        [JsonProperty(PropertyName = "description")]
        [DefaultValue("")]
        public string Description { get; set; }


        /// <summary>
        /// Loan disbursement, deposit, or guarantee opening - principal balance increase
        /// </summary>
        /// <value>Loan disbursement, deposit, or guarantee opening - principal balance increase</value>
        [JsonProperty(PropertyName = "disbursement")]
        [DefaultValue(0.0)]
        public double? Disbursement { get; set; }


        /// <summary>
        /// Principal balance before payment
        /// </summary>
        /// <value>Principal balance before payment</value>
        [JsonProperty(PropertyName = "starting-balance")]
        [DefaultValue(0.0)]
        public double? StartingBalance { get; set; }


        /// <summary>
        /// Principal amount to be repayed.
        /// </summary>
        /// <value>Principal amount to be repayed.</value>
        [JsonProperty(PropertyName = "principal-repayment")]
        public double? PrincipalRepayment { get; set; }


        /// <summary>
        /// Interest amount to be payed
        /// </summary>
        /// <value>Interest amount to be payed</value>
        [JsonProperty(PropertyName = "interest-repayment")]
        public double? InterestRepayment { get; set; }


        /// <summary>
        /// Sum of principal and interest to be repayed
        /// </summary>
        /// <value>Sum of principal and interest to be repayed</value>
        [JsonProperty(PropertyName = "annuity")]
        public double? Annuity { get; set; }


        /// <summary>
        /// Principal balance after payment
        /// </summary>
        /// <value>Principal balance after payment</value>
        [JsonProperty(PropertyName = "outstanding-balance")]
        [DefaultValue(0.0)]
        public double? OutstandingBalance { get; set; }


        /// <summary>
        /// Fee payment
        /// </summary>
        /// <value>Fee payment</value>
        [JsonProperty(PropertyName = "fee")]
        [DefaultValue(0.0)]
        public double? Fee { get; set; }


        /// <summary>
        /// Other expenses
        /// </summary>
        /// <value>Other expenses</value>
        [JsonProperty(PropertyName = "other-expenses")]
        [DefaultValue(0.0)]
        public double? OtherExpenses { get; set; }


        /// <summary>
        /// Positive and negative flows of cash collateral
        /// </summary>
        /// <value>Positive and negative flows of cash collateral</value>
        [JsonProperty(PropertyName = "cash-collateral")]
        [DefaultValue(0.0)]
        public double? CashCollateral { get; set; }


        /// <summary>
        /// Part of payment that is being delayed for some future date. If such defferal exists, then the same amount must be deducted from principal repayment.
        /// </summary>
        /// <value>Part of payment that is being delayed for some future date. If such defferal exists, then the same amount must be deducted from principal repayment.</value>
        [JsonProperty(PropertyName = "deferred-payment")]
        [DefaultValue(0.0)]
        public double? DeferredPayment { get; set; }


        /// <summary>
        /// Repayment of aggregated deferred payments
        /// </summary>
        /// <value>Repayment of aggregated deferred payments</value>
        [JsonProperty(PropertyName = "deferred-payment-installment")]
        [DefaultValue(0.0)]
        public double? DeferredPaymentInstallment { get; set; }


        /// <summary>
        /// Total net cash flow between the bank and the customer
        /// </summary>
        /// <value>Total net cash flow between the bank and the customer</value>
        [JsonProperty(PropertyName = "net-cash-flow")]
        [DefaultValue(0.0)]
        public double? NetCashFlow { get; set; }


        /// <summary>
        /// Net cash flow that is being discounted to present value using calculated EAPR
        /// </summary>
        /// <value>Net cash flow that is being discounted to present value using calculated EAPR</value>
        [JsonProperty(PropertyName = "discounted-net-cash-flow")]
        [DefaultValue(0.0)]
        public double? DiscountedNetCashFlow { get; set; }


        /// <summary>
        /// Disbursement flows discounted using APR counted based on discounted-net-flow
        /// </summary>
        /// <value>Disbursement flows discounted using APR counted based on discounted-net-flow</value>
        [JsonProperty(PropertyName = "discounted-disbursement")]
        [DefaultValue(0.0)]
        public double? DiscountedDisbursement { get; set; }


        /// <summary>
        /// Cash collateral flows discounted using APR counted based on discounted-net-flow
        /// </summary>
        /// <value>Cash collateral flows discounted using APR counted based on discounted-net-flow</value>
        [JsonProperty(PropertyName = "discounted-cash-collateral-flow")]
        [DefaultValue(0.0)]
        public double? DiscountedCashCollateralFlow { get; set; }



        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class InstallmentPlanRow {\n");
            sb.Append("  Ordinal: ").Append(Ordinal).Append("\n");
            sb.Append("  Date: ").Append(Date).Append("\n");
            sb.Append("  ActivityKind: ").Append(ActivityKind).Append("\n");
            sb.Append("  Description: ").Append(Description).Append("\n");
            sb.Append("  Disbursement: ").Append(Disbursement).Append("\n");
            sb.Append("  StartingBalance: ").Append(StartingBalance).Append("\n");
            sb.Append("  PrincipalRepayment: ").Append(PrincipalRepayment).Append("\n");
            sb.Append("  InterestRepayment: ").Append(InterestRepayment).Append("\n");
            sb.Append("  Annuity: ").Append(Annuity).Append("\n");
            sb.Append("  OutstandingBalance: ").Append(OutstandingBalance).Append("\n");
            sb.Append("  Fee: ").Append(Fee).Append("\n");
            sb.Append("  OtherExpenses: ").Append(OtherExpenses).Append("\n");
            sb.Append("  CashCollateral: ").Append(CashCollateral).Append("\n");
            sb.Append("  DeferredPayment: ").Append(DeferredPayment).Append("\n");
            sb.Append("  DeferredPaymentInstallment: ").Append(DeferredPaymentInstallment).Append("\n");
            sb.Append("  NetCashFlow: ").Append(NetCashFlow).Append("\n");
            sb.Append("  DiscountedNetCashFlow: ").Append(DiscountedNetCashFlow).Append("\n");
            sb.Append("  DiscountedDisbursement: ").Append(DiscountedDisbursement).Append("\n");
            sb.Append("  DiscountedCashCollateralFlow: ").Append(DiscountedCashCollateralFlow).Append("\n");

            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="obj">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((InstallmentPlanRow)obj);
        }

        /// <summary>
        /// Returns true if InstallmentPlanRow instances are equal
        /// </summary>
        /// <param name="other">Instance of InstallmentPlanRow to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(InstallmentPlanRow other)
        {

            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return
                (
                    this.Ordinal == other.Ordinal ||
                    this.Ordinal != null &&
                    this.Ordinal.Equals(other.Ordinal)
                ) &&
                (
                    this.Date == other.Date ||
                    this.Date != null &&
                    this.Date.Equals(other.Date)
                ) &&
                (
                    this.ActivityKind == other.ActivityKind ||
                    this.ActivityKind != null &&
                    this.ActivityKind.Equals(other.ActivityKind)
                ) &&
                (
                    this.Description == other.Description ||
                    this.Description != null &&
                    this.Description.Equals(other.Description)
                ) &&
                (
                    this.Disbursement == other.Disbursement ||
                    this.Disbursement != null &&
                    this.Disbursement.Equals(other.Disbursement)
                ) &&
                (
                    this.StartingBalance == other.StartingBalance ||
                    this.StartingBalance != null &&
                    this.StartingBalance.Equals(other.StartingBalance)
                ) &&
                (
                    this.PrincipalRepayment == other.PrincipalRepayment ||
                    this.PrincipalRepayment != null &&
                    this.PrincipalRepayment.Equals(other.PrincipalRepayment)
                ) &&
                (
                    this.InterestRepayment == other.InterestRepayment ||
                    this.InterestRepayment != null &&
                    this.InterestRepayment.Equals(other.InterestRepayment)
                ) &&
                (
                    this.Annuity == other.Annuity ||
                    this.Annuity != null &&
                    this.Annuity.Equals(other.Annuity)
                ) &&
                (
                    this.OutstandingBalance == other.OutstandingBalance ||
                    this.OutstandingBalance != null &&
                    this.OutstandingBalance.Equals(other.OutstandingBalance)
                ) &&
                (
                    this.Fee == other.Fee ||
                    this.Fee != null &&
                    this.Fee.Equals(other.Fee)
                ) &&
                (
                    this.OtherExpenses == other.OtherExpenses ||
                    this.OtherExpenses != null &&
                    this.OtherExpenses.Equals(other.OtherExpenses)
                ) &&
                (
                    this.CashCollateral == other.CashCollateral ||
                    this.CashCollateral != null &&
                    this.CashCollateral.Equals(other.CashCollateral)
                ) &&
                (
                    this.DeferredPayment == other.DeferredPayment ||
                    this.DeferredPayment != null &&
                    this.DeferredPayment.Equals(other.DeferredPayment)
                ) &&
                (
                    this.DeferredPaymentInstallment == other.DeferredPaymentInstallment ||
                    this.DeferredPaymentInstallment != null &&
                    this.DeferredPaymentInstallment.Equals(other.DeferredPaymentInstallment)
                ) &&
                (
                    this.NetCashFlow == other.NetCashFlow ||
                    this.NetCashFlow != null &&
                    this.NetCashFlow.Equals(other.NetCashFlow)
                ) &&
                (
                    this.DiscountedNetCashFlow == other.DiscountedNetCashFlow ||
                    this.DiscountedNetCashFlow != null &&
                    this.DiscountedNetCashFlow.Equals(other.DiscountedNetCashFlow)
                ) &&
                (
                    this.DiscountedDisbursement == other.DiscountedDisbursement ||
                    this.DiscountedDisbursement != null &&
                    this.DiscountedDisbursement.Equals(other.DiscountedDisbursement)
                ) &&
                (
                    this.DiscountedCashCollateralFlow == other.DiscountedCashCollateralFlow ||
                    this.DiscountedCashCollateralFlow != null &&
                    this.DiscountedCashCollateralFlow.Equals(other.DiscountedCashCollateralFlow)
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            // credit: http://stackoverflow.com/a/263416/677735
            unchecked // Overflow is fine, just wrap
            {
                int hash = 41;
                // Suitable nullity checks etc, of course :)

                if (this.Ordinal != null)
                    hash = hash * 59 + this.Ordinal.GetHashCode();

                if (this.Date != null)
                    hash = hash * 59 + this.Date.GetHashCode();

                if (this.ActivityKind != null)
                    hash = hash * 59 + this.ActivityKind.GetHashCode();

                if (this.Description != null)
                    hash = hash * 59 + this.Description.GetHashCode();

                if (this.Disbursement != null)
                    hash = hash * 59 + this.Disbursement.GetHashCode();

                if (this.StartingBalance != null)
                    hash = hash * 59 + this.StartingBalance.GetHashCode();

                if (this.PrincipalRepayment != null)
                    hash = hash * 59 + this.PrincipalRepayment.GetHashCode();

                if (this.InterestRepayment != null)
                    hash = hash * 59 + this.InterestRepayment.GetHashCode();

                if (this.Annuity != null)
                    hash = hash * 59 + this.Annuity.GetHashCode();

                if (this.OutstandingBalance != null)
                    hash = hash * 59 + this.OutstandingBalance.GetHashCode();

                if (this.Fee != null)
                    hash = hash * 59 + this.Fee.GetHashCode();

                if (this.OtherExpenses != null)
                    hash = hash * 59 + this.OtherExpenses.GetHashCode();

                if (this.CashCollateral != null)
                    hash = hash * 59 + this.CashCollateral.GetHashCode();

                if (this.DeferredPayment != null)
                    hash = hash * 59 + this.DeferredPayment.GetHashCode();

                if (this.DeferredPaymentInstallment != null)
                    hash = hash * 59 + this.DeferredPaymentInstallment.GetHashCode();

                if (this.NetCashFlow != null)
                    hash = hash * 59 + this.NetCashFlow.GetHashCode();

                if (this.DiscountedNetCashFlow != null)
                    hash = hash * 59 + this.DiscountedNetCashFlow.GetHashCode();

                if (this.DiscountedDisbursement != null)
                    hash = hash * 59 + this.DiscountedDisbursement.GetHashCode();

                if (this.DiscountedCashCollateralFlow != null)
                    hash = hash * 59 + this.DiscountedCashCollateralFlow.GetHashCode();

                return hash;
            }
        }

        #region Operators

        public static bool operator ==(InstallmentPlanRow left, InstallmentPlanRow right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(InstallmentPlanRow left, InstallmentPlanRow right)
        {
            return !Equals(left, right);
        }

        #endregion Operators
    }
}
