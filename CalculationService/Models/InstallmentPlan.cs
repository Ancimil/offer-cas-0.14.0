using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CalculationService.Models
{
    public partial class InstallmentPlan : IEquatable<InstallmentPlan>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InstallmentPlan" /> class.
        /// </summary>
        /// <param name="NumberOfInstallments">NumberOfInstallments.</param>
        /// <param name="Annuity">Annuity.</param>
        /// <param name="EffectiveInterestRate">EffectiveInterestRate.</param>
        /// <param name="Installments">Installment plan rows.</param>
        public InstallmentPlan(double? NumberOfInstallments = null, double? Annuity = null, double? EffectiveInterestRate = null, List<InstallmentPlanRow> Installments = null)
        {
            this.NumberOfInstallments = NumberOfInstallments;
            this.Annuity = Annuity;
            this.EffectiveInterestRate = EffectiveInterestRate;
            this.Installments = Installments;

        }


        /// <summary>
        /// Gets or Sets NumberOfInstallments
        /// </summary>
        [JsonProperty(PropertyName = "number-of-installments")]
        public double? NumberOfInstallments { get; set; }


        /// <summary>
        /// Gets or Sets Annuity
        /// </summary>
        [JsonProperty(PropertyName = "annuity")]
        public double? Annuity { get; set; }


        /// <summary>
        /// Gets or Sets EffectiveInterestRate
        /// </summary>
        [JsonProperty(PropertyName = "effective-interest-rate")]
        public double? EffectiveInterestRate { get; set; }


        /// <summary>
        /// Installment plan rows
        /// </summary>
        /// <value>Installment plan rows</value>
        [JsonProperty(PropertyName = "installments")]
        public List<InstallmentPlanRow> Installments { get; set; }



        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class InstallmentPlan {\n");
            sb.Append("  NumberOfInstallments: ").Append(NumberOfInstallments).Append("\n");
            sb.Append("  Annuity: ").Append(Annuity).Append("\n");
            sb.Append("  EffectiveInterestRate: ").Append(EffectiveInterestRate).Append("\n");
            sb.Append("  Installments: ").Append(Installments).Append("\n");

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
            return Equals((InstallmentPlan)obj);
        }

        /// <summary>
        /// Returns true if InstallmentPlan instances are equal
        /// </summary>
        /// <param name="other">Instance of InstallmentPlan to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(InstallmentPlan other)
        {

            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return
                (
                    this.NumberOfInstallments == other.NumberOfInstallments ||
                    this.NumberOfInstallments != null &&
                    this.NumberOfInstallments.Equals(other.NumberOfInstallments)
                ) &&
                (
                    this.Annuity == other.Annuity ||
                    this.Annuity != null &&
                    this.Annuity.Equals(other.Annuity)
                ) &&
                (
                    this.EffectiveInterestRate == other.EffectiveInterestRate ||
                    this.EffectiveInterestRate != null &&
                    this.EffectiveInterestRate.Equals(other.EffectiveInterestRate)
                ) &&
                (
                    this.Installments == other.Installments ||
                    this.Installments != null &&
                    this.Installments.SequenceEqual(other.Installments)
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

                if (this.NumberOfInstallments != null)
                    hash = hash * 59 + this.NumberOfInstallments.GetHashCode();

                if (this.Annuity != null)
                    hash = hash * 59 + this.Annuity.GetHashCode();

                if (this.EffectiveInterestRate != null)
                    hash = hash * 59 + this.EffectiveInterestRate.GetHashCode();

                if (this.Installments != null)
                    hash = hash * 59 + this.Installments.GetHashCode();

                return hash;
            }
        }

        #region Operators

        public static bool operator ==(InstallmentPlan left, InstallmentPlan right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(InstallmentPlan left, InstallmentPlan right)
        {
            return !Equals(left, right);
        }

        #endregion Operators

    }

    public class Test
    {
        
    }
}
