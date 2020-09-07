using Offer.Domain.AggregatesModel.ApplicationAggregate;
using PriceCalculation.Models.Pricing;
using Offer.Domain.Calculations;
using System;
using System.Collections.Generic;
using System.Text;
using PriceCalculation.Models.Product;

namespace OfferApiTest
{
    public class Utility
    {

        public static Application GetApplication(ProductConditions conditions)
        {
            return new  Application
            {
                ArrangementNumber = "1",
                CustomerName = "Tester",
                StatusInformation = new StatusInformation
                {
                    Description = "Works as a tester"
                },
                CustomerSegment = "student",
                CollateralModel = "two-co-debtors",
                
                RiskScore = 55,
                ChannelCode = "web",
                RequestDate = DateTime.Now,
                ArrangementRequests = new List<ArrangementRequest>
                {
                    GetArangementRequest(conditions)
                }
            };
        }
        public static PriceCalculationParameters GetPriceCalculationParametersFromConditions(ProductConditions conditions)
        {
            return GetPriceCalculationParameterFromTermLoanRequest(GetArangementRequest(conditions));
        }

        public static VariationDefinitionParams GetVariationDefinitionParamsFromApplication(Application application)
        {
            return new VariationDefinitionParams
            {
                Channel = application.ChannelCode,
                PartOfBundle = application.ProductCode,
                RiskScore = application.RiskScore,
                CustomerSegment = application.CustomerSegment,
                Amount = (application.ArrangementRequests[0] as TermLoanRequest).Amount,
                // Should be string
                Term = 30,//int.Parse((application.ArrangementRequests[0] as TermLoanRequest).Term),
                Currency = (application.ArrangementRequests[0] as TermLoanRequest).Currency,
                
                // Bind rest of the parameters CollateralModel, RuleCurrency
            };
        }

        public static TermLoanRequest GetArangementRequest(ProductConditions conditions)
        {
            var currency = "RSD";
            if (conditions.InterestRates != null && conditions.InterestRates.Count > 0)
            {
                currency = conditions.InterestRates[0].Currencies[0];
            }
            if (conditions.Fees != null && conditions.Fees.Count > 0)
            {
                currency = conditions.Fees[0].Currencies[0];
            }
            return new TermLoanRequest()
            {
                Amount = 50000,
                Napr = 10,
                Term = "P30M",
                Annuity = 325,
                Currency = currency,
                DownpaymentAmount = 10000,
                CalculationDate = DateTime.Today,
                ProductSnapshot = new ProductSnapshot
                {
                    Conditions = conditions
                }

            };
        }

        public static PriceCalculationParameters GetPriceCalculationParameterFromTermLoanRequest(TermLoanRequest request)
        {
            return new PriceCalculationParameters
            {
                Amount = request.Amount,
                Currency = request.Currency,
                Fees = request.Conditions?.Fees,
                InterestRates = request.Conditions?.InterestRates,
                Term = request.Term,
                RequestDate = DateTime.Today,
                // Check how are rest of the parameters mapped
            };
        }
    }
}
