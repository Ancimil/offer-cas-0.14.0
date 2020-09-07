using Newtonsoft.Json;
using MicroserviceCommon.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using PriceCalculation.Models.Pricing;
using Newtonsoft.Json.Linq;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public class Application : IAggregateRoot
    {
        public Application()
        {
            StatusInformation = new StatusInformation();
        }
        [Required]
        [JsonIgnore]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ApplicationId { get; set; }

        [NotMapped]
        public string ApplicationNumber
        {
            get
            {
                var result = "0000000000" + ApplicationId;
                return result.Substring(result.Length - 10);
            }
        }

        [JsonIgnore]
        [Column("ExposureInfo")]
        public string _ExposureInfo
        {
            get
            {
                return ExposureInfo == null ? null : JsonConvert.SerializeObject(ExposureInfo);
            }
            set
            {
                ExposureInfo = value == null ? null : DeserialzeExposure(value);
            }
        }

        public ExposureInfo DeserialzeExposure(string value)
        {
            try { return JsonConvert.DeserializeObject<ExposureInfo>(value); } catch { return null; }
        }

        [NotMapped]
        public ExposureInfo ExposureInfo { get; set; }
        [JsonIgnore]
        public string _Extended
        {
            get
            {
                return Extended == null ? null : JsonConvert.SerializeObject(Extended);
            }
            set
            {
                Extended = value == null ? null : JsonConvert.DeserializeObject<IDictionary<string, IDictionary<string, JToken>>>(value);
            }
        }
        [NotMapped]
        public IDictionary<string, IDictionary<string, JToken>> Extended { get; set; }

        [Required]
        public ApplicationStatus Status { get; set; }

        // Open classification for Milestones
        public string Phase { get; set; }

        [MaxLength(128)]
        public string ProductCode { get; set; } // Product which has been selected in product catalog. There might be other products implicitely open through onboarding (such as current account) and also there might be other products in the bundle.

        [MaxLength(128)]
        public string ProductName { get; set; }

        [MaxLength(256)]
        public string CustomerNumber { get; set; }

        [MaxLength(256)]
        public string CustomerName { get; set; }

        [MaxLength(256)]
        public string CustomerSegment { get; set; }

        public string OrganizationUnitCode { get; set; }

        [JsonIgnore]
        public OrganizationUnit OrganizationUnit { get; set; }

        [MaxLength(128)]
        public string ChannelCode { get; set; } // Application creation channel

        [MaxLength(1024)]
        public string PortfolioId { get; set; }

        [MaxLength(128)]
        public string CampaignCode { get; set; } // Also promotional code
        public long? LeadId { get; set; }

        [MaxLength(256)]
        public string DecisionNumber { get; set; }

        [MaxLength(128)]
        public string Initiator { get; set; }

        [MaxLength(128)]
        public string CountryCode { get; set; }

        [MaxLength(128)]
        public string PrefferedCulture { get; set; }

        [MaxLength(256)]
        public string SigningOption { get; set; }

        [MaxLength(256)]
        public string CollateralModel { get; set; }

        public decimal? RiskScore { get; set; }
        public string CreditRating { get; set; }
        public decimal? CustomerValue { get; set; }

        public StatusInformation StatusInformation { get; set; }

        public DateTime? RequestDate { get; set; }

        public DateTime? ExpirationDate { get; set; }

        public string CancelationReason { get; set; }

        public string CancelationComment { get; set; }

        public DateTime? Created { get; set; }

        public DateTime? StatusChangeDate { get; set; }

        public DateTime? LastModified { get; set; }

        public string CreatedByName { get; set; }

        public List<ArrangementRequest> ArrangementRequests { get; set; }

        [Required]
        public List<Party> InvolvedParties { get; set; }

        [Required]
        public List<Questionnaire> Questionnaires { get; set; }

        public List<ApplicationDocument> Documents { get; set; }

        public bool TermLimitBreached { get; set; }

        public bool AmountLimitBreached { get; set; }

        public bool PreferencialPrice { get; set; }

        public decimal? LoanToValue { get; set; }
        public decimal? MaximalAnnuity { get; set; }
        public decimal? MaximalAmount { get; set; }
        public decimal? DebtToIncome { get; set; }
        public decimal? CustomerRemainingAbilityToPay { get; set; }
        public decimal? EffectiveRemainingAbilityToPay { get; set; }
        public bool IsPreApproved { get; set; } = false;
        public string PreApprovalType { get; set; }
        public bool? OriginatesBundle { get; set; } = false;
        public bool? CustomerApplied { get; set; }
        [JsonIgnore]
        public string _AvailableProducts
        {
            get
            {
                return AvailableProducts == null ? null : string.Join(",", AvailableProducts.ToArray());
            }
            set
            {
                AvailableProducts = value == null ? null : value.Split(",").ToList();
            }
        }
        [NotMapped]
        public List<string> AvailableProducts { get; set; }

        public PriceCalculationParameters GetPriceCalculationParameters(string productCode)
        {
            var defaultArrangementRequest = ArrangementRequests?
                .Where(x => x.ProductCode.Equals(ProductCode)).FirstOrDefault();

            var priceCalc = new PriceCalculationParameters
            {
                RequestDate = RequestDate,
                Channel = ChannelCode,
                RiskScore = RiskScore,
                CustomerSegment = CustomerSegment,
                CustomerValue = CustomerValue,
                CreditRating = CreditRating,
                DebtToIncome = DebtToIncome
        };
            if (defaultArrangementRequest != null && 
                defaultArrangementRequest.ProductSnapshot != null && 
                defaultArrangementRequest.ProductSnapshot.IsPackage)
            {
                priceCalc.AdditionalProperties = ArrangementRequests?
                    .Where(r => !r.ProductCode.Equals(ProductCode))
                    .Select(b => new { b.ProductCode, Enabled = b.Enabled ?? false })
                    .Distinct()
                    .ToDictionary(k => k.ProductCode, v => (JToken)v.Enabled);
            }
            return priceCalc;
        }
    }

    [ComplexType]
    public class StatusInformation
    {
        [MaxLength(1024)]
        public string Description { get; set; } // html for displaying status to customer
        public string Title { get; set; } // html for displaying status to customer
        public string Html { get; set; }
    }

}
