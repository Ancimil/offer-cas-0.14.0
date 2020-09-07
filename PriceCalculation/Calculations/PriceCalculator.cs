using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssecoCurrencyConvertion;
using System.Globalization;
using MicroserviceCommon.ApiUtil;
using MicroserviceCommon.Services;
using System.Runtime.Serialization;
using PriceCalculation.Services;
using PriceCalculation.Models.Pricing;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
namespace PriceCalculation.Calculations
{
    public class PriceCalculator
    {
        private readonly IPriceCalculationService priceCalculationService;
        private readonly IMarketRatesService marketRatesService;
        private readonly IConfigurationService configurationService;
        private readonly List<PriceVariationOrigins> notPreferentialVariationOrigins =
            new List<PriceVariationOrigins> { PriceVariationOrigins.Product, PriceVariationOrigins.ProductOptions,
                PriceVariationOrigins.Campaign };

        public PriceCalculator(IPriceCalculationService priceCalculationService, IMarketRatesService marketRatesService,
            IConfigurationService configurationService)
        {
            this.priceCalculationService = priceCalculationService;
            this.marketRatesService = marketRatesService;
            this.configurationService = configurationService;
        }

        public async Task<PriceCalculationResult> CalculatePrice(PriceCalculationParameters calculationParameters)
        {
            var definitionParams = GetVariationDefinitionParams(calculationParameters);
            var resultChanged = false;
            var ratesResult = await CalculateInterestRates(calculationParameters, definitionParams);
            var feesResult = await CalculateFees(calculationParameters, definitionParams);

            return new PriceCalculationResult
            {
                InterestRates = ratesResult.Rates,
                Fees = feesResult.Fees,
                Napr = ratesResult.Napr,
                ResultChanged = resultChanged || ratesResult.ResultChanged || feesResult.ResultChanged
            };
        }

        public bool HasPreferentialPrice(Conditions conditions)
        {

            if (conditions?.InterestRates != null)
            {
                var arrRates = conditions.InterestRates;
                foreach (InterestRateCondition rate in arrRates)
                {
                    if (rate.Variations?.FirstOrDefault(v => !notPreferentialVariationOrigins.Contains(v.Origin)) != null ||
                        rate.UpperLimitVariations?.FirstOrDefault(v => !notPreferentialVariationOrigins.Contains(v.Origin)) != null ||
                        rate.LowerLimitVariations?.FirstOrDefault(v => !notPreferentialVariationOrigins.Contains(v.Origin)) != null)
                    {
                        return true;
                    }
                }
            }
            if (conditions?.Fees != null)
            {
                var arrFees = conditions.Fees;
                foreach (FeeCondition fee in arrFees)
                {
                    if (fee.Variations?.FirstOrDefault(v => !notPreferentialVariationOrigins.Contains(v.Origin)) != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private decimal? GetAmountInRuleCurrency(PricingCondition condition, string currency, decimal amount)
        {
            if (string.IsNullOrEmpty(currency) || currency.Equals(condition.PricingRuleCurrency) || string.IsNullOrEmpty(condition.PricingRuleCurrency))
            {
                return amount;
            }
            else
            {
                CurrencyConverter currencyConverter = new CurrencyConverter();
                var currencyConversionMethod = configurationService.GetEffective("offer/currency-conversion-method").Result;
                return currencyConverter.CurrencyConvert(amount, currency, condition.PricingRuleCurrency,
                    DateTime.Today.ToString("o", CultureInfo.InvariantCulture), currencyConversionMethod);
            }
        }

        private decimal? CalculateRate(InterestRate rate, DateTime requestDate)
        {
            if (rate == null)
            {
                return null;
            }
            var baseRateValue = GetBaseRateValue(rate.BaseRateId, requestDate);
            rate.BaseRateValue = baseRateValue;
            if (rate.SpreadRateValue.HasValue)
            {
                return rate.SpreadRateValue.Value + (baseRateValue ?? 0);
            }
            else
            {
                return baseRateValue;
            }
        }

        private decimal GetPrice(InterestRateCondition rate, DateTime requestDate)
        {
            // Main rate needs to have a value
            var tempR = CalculateRate(rate.Rate, requestDate);
            rate.CalculatedRate = (tempR ?? 0) + (SumInterestRateVariations(rate.Variations) ?? 0);
            var lowerRate = CalculateRate(rate.LowerLimit, requestDate);
            if (lowerRate.HasValue)
            {
                rate.CalculatedLowerLimit = lowerRate + SumInterestRateVariations(rate.LowerLimitVariations) ?? 0;
                if (rate.CalculatedRate < rate.CalculatedLowerLimit)
                {
                    rate.CalculatedRate = rate.CalculatedLowerLimit.Value;
                    rate.LowerLimitApplied = true;
                }
            }
            var upperRate = CalculateRate(rate.UpperLimit, requestDate);
            if (upperRate.HasValue)
            {
                rate.CalculatedUpperLimit = upperRate + SumInterestRateVariations(rate.UpperLimitVariations) ?? 0;
                if (rate.CalculatedRate > rate.CalculatedUpperLimit)
                {
                    rate.CalculatedRate = rate.CalculatedUpperLimit.Value;
                    rate.UpperLimitApplied = true;
                }
            }
            return rate.CalculatedRate;
        }

        private decimal? SumInterestRateVariations(List<InterestRateVariation> variations)
        {
            if (variations == null || variations.Count() == 0)
            {
                // return null;
                return 0;
            }
            decimal sum = 0;
            foreach (InterestRateVariation variation in variations)
            {
                sum += variation.Percentage;
            }
            return sum;
        }

        private FeeCondition SolveFeeCondition(FeeCondition fee)
        {
            if (fee != null)
            {
                FeeVariation feeVariation = SumFeeVariations(fee.Variations);
                fee.CalculatedFixedAmount =(fee.FixedAmount?.Amount ?? 0) + feeVariation.FixedAmount;
                fee.CalculatedLowerLimit = (fee.LowerLimit?.Amount ?? 0) + feeVariation.LowerLimit;
                fee.CalculatedUpperLimit = (fee.UpperLimit?.Amount ?? 0) + feeVariation.UpperLimit;
                fee.CalculatedPercentage = fee.Percentage + feeVariation.Percentage;

                if (fee.PercentageLowerLimit.HasValue && fee.PercentageLowerLimit.Value > fee.CalculatedPercentage)
                {
                    fee.CalculatedPercentage = fee.PercentageLowerLimit.Value;
                    fee.PercentageLowerLimitApplied = true;
                }
                else if (fee.PercentageUpperLimit.HasValue && fee.CalculatedPercentage > fee.PercentageUpperLimit.Value)
                {
                    fee.CalculatedPercentage = fee.PercentageUpperLimit.Value;
                    fee.PercentageUpperLimitApplied = true;
                }
            }
            return fee;
        }

        private FeeVariation SumFeeVariations(List<FeeVariation> variations)
        {
            var fee = new FeeVariation
            {
                Percentage = 0,
                FixedAmount = 0,
                LowerLimit = 0,
                UpperLimit = 0
            };
            if (variations != null)
            {
                foreach (FeeVariation variation in variations)
                {
                    fee.Percentage += variation.Percentage;
                    fee.FixedAmount += variation.FixedAmount;
                    fee.LowerLimit += variation.LowerLimit;
                    fee.UpperLimit += variation.UpperLimit;
                }
            }
            return fee;
        }

        private VariationDefinitionParams GetVariationDefinitionParams(PriceCalculationParameters parameters)
        {
            var defaultParamsString = configurationService.GetEffective("offer/price-calculation/default-parameters").Result;
            var defaultParams = (VariationDefinitionParams)CaseUtil.ConvertFromJsonToObject(defaultParamsString, typeof(VariationDefinitionParams));
            var defParams = new VariationDefinitionParams
            {
                Channel = parameters.Channel ?? defaultParams.Channel,
                Amount = parameters.Amount,
                Currency = parameters.Currency
            };
            defParams.CustomerSegment = parameters.CustomerSegment ?? defaultParams.CustomerSegment;
            defParams.RiskScore = parameters.RiskScore ?? defaultParams.RiskScore;
            defParams.PartOfBundle = parameters.PartOfBundle ?? defaultParams.PartOfBundle;
            defParams.CollateralModel = parameters.CollateralModel ?? defaultParams.CollateralModel;
            defParams.HasCampaignInculded = parameters.Campaign != null;
            defParams.DebtToIncome = parameters.DebtToIncome ?? defaultParams.DebtToIncome;
            defParams.CustomerValue = parameters.CustomerValue ?? defaultParams.CustomerValue;
            defParams.CreditRating = parameters.CreditRating ?? defaultParams.CreditRating;
            var bundledComponentns = parameters.AdditionalProperties ?? defaultParams.AdditionalProperties;
            defParams.AdditionalProperties = new Dictionary<string, JToken>();
            var jsonData = JsonConvert.SerializeObject(bundledComponentns);
            if (jsonData.Equals("{}")) {
              Console.WriteLine(
                new System.Diagnostics.StackTrace().ToString()
                );   
            }
            foreach (var item in bundledComponentns)
            {
                defParams.AdditionalProperties.Add("bundledComponent_" + item.Key, item.Value);
            }
            try
            {
                defParams.Term = Utility.GetMonthsFromPeriod(parameters.Term);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                defParams.Term = 0;
            }
            return defParams;
        }

        private async Task<ResolvedRatesResult> CalculateInterestRates(PriceCalculationParameters calculationParameters, VariationDefinitionParams definitionParams)
        {
            var bundleVariationGroup = await configurationService
                .GetEffective("offer/price-calculation/bundle-variation-group", "PRODUCT-BUNDLING-DISCOUNT");
            bundleVariationGroup = bundleVariationGroup.ToLower();

            var result = new ResolvedRatesResult
            {
                ResultChanged = false
            };
            if (calculationParameters.InterestRates != null)
            {
                result.Rates = new List<InterestRateCondition>();
                var arrRates = calculationParameters.InterestRates;
                decimal napr = -1;
                foreach (InterestRateCondition rate in arrRates)
                {
                    var oldCalculatedRate = rate.CalculatedRate;
                    if (
                        // check if rate is defined at all
                        rate == null ||
                        // check if rate is effective on request date
                        rate.EffectiveDate > calculationParameters.RequestDate ||
                        // check if rate is defined for main currency
                        (rate.Currencies != null && !rate.Currencies.Contains(calculationParameters.Currency)) ||
                        // check if rate is defined for scheduled periods (if scheduled periods exist at all)
                        (string.IsNullOrEmpty(rate.Periods) &&
                        calculationParameters.ScheduledPeriods != null && calculationParameters.ScheduledPeriods.Count() > 0)
                        ||
                        (!string.IsNullOrEmpty(rate.Periods) &&
                        (calculationParameters.ScheduledPeriods == null || calculationParameters.ScheduledPeriods.Count() == 0))
                        ||
                        (!string.IsNullOrEmpty(rate.Periods) &&
                        calculationParameters.ScheduledPeriods != null && calculationParameters.ScheduledPeriods.Count() > 0 &&
                        rate.Periods.Replace(" ", "").Split(",").Intersect(calculationParameters.ScheduledPeriods.Select(p => p.PeriodType)).Count() == 0))
                    {
                        continue;
                    }
                    if (calculationParameters.Amount.HasValue)
                    {
                        definitionParams.AmountInRuleCurrency = GetAmountInRuleCurrency(rate, calculationParameters.Currency,
                            calculationParameters.Amount.Value);
                    }

                    var newRate = rate.Copy();
                    newRate = ResolveRateBenefits(calculationParameters, newRate);
                    newRate = ResolveRateOptions(calculationParameters, newRate);

                    if (!string.IsNullOrEmpty(newRate.VariationsDefinitionDMN))
                    {
                        var newVars = await priceCalculationService.ResolveInterestRateVariationDmn(newRate.VariationsDefinitionDMN, definitionParams, newRate.VariationsDefinition);
                        if (newVars != null && newVars.Count > 0)
                        {
                            if (newRate.Variations != null || newRate.Variations.Count > 0)
                            {
                                newRate.Variations = newRate.Variations.Where(nv => nv.Origin != PriceVariationOrigins.Product).ToList();
                            }
                            else
                            {
                                newRate.Variations = new List<InterestRateVariation>();
                            }
                            newRate.Variations.AddRange(newVars);
                        }
                    }
                    if (!string.IsNullOrEmpty(newRate.UpperLimitVariationsDefinitionDMN))
                    {
                        var newVars = await priceCalculationService.ResolveInterestRateVariationDmn(newRate.UpperLimitVariationsDefinitionDMN, definitionParams, newRate.UpperLimitVariationsDefinition);
                        if (newVars != null && newVars.Count > 0)
                        {
                            if (newRate.UpperLimitVariations != null || newRate.UpperLimitVariations.Count > 0)
                            {
                                newRate.UpperLimitVariations = newRate.UpperLimitVariations.Where(nv => nv.Origin != PriceVariationOrigins.Product).ToList();
                            }
                            else
                            {
                                newRate.UpperLimitVariations = new List<InterestRateVariation>();
                            }
                            newRate.UpperLimitVariations.AddRange(newVars);
                        }
                    }
                    if (!string.IsNullOrEmpty(newRate.LowerLimitVariationsDefinitionDMN))
                    {
                        var newVars = await priceCalculationService.ResolveInterestRateVariationDmn(newRate.LowerLimitVariationsDefinitionDMN, definitionParams, newRate.LowerLimitVariationsDefinition);
                        if (newVars != null && newVars.Count > 0)
                        {
                            if (newRate.LowerLimitVariations != null || newRate.LowerLimitVariations.Count > 0)
                            {
                                newRate.LowerLimitVariations = newRate.LowerLimitVariations.Where(nv => nv.Origin != PriceVariationOrigins.Product).ToList();
                            }
                            else
                            {
                                newRate.LowerLimitVariations = new List<InterestRateVariation>();
                            }
                            newRate.LowerLimitVariations.AddRange(newVars);
                        }
                    }

                    var newPrice = GetPrice(newRate, calculationParameters.RequestDate ?? DateTime.Today);
                    if (newPrice != oldCalculatedRate)
                    {
                        result.ResultChanged = true;
                    }

                    var rateWoBundle = GetInterestRateWithoutBundle(newRate, bundleVariationGroup, calculationParameters.RequestDate);
                    if (rateWoBundle > 0)
                    {
                        newRate.RateWithoutBundle = rateWoBundle;
                    }

                    if (newRate.Variations.Any(v => v.Origin == PriceVariationOrigins.SalesDiscount))
                    {
                        newRate.Variations.Reverse();
                    }

                    result.Rates.Add(newRate);
                    if (newRate.Kind == InterestRateKinds.RegularInterest && napr < 0)
                    {
                        napr = newRate.CalculatedRate;
                        result.Napr = napr;
                    }
                }
            }
            return result;
        }

        private InterestRateCondition ResolveRateOptions(PriceCalculationParameters calculationParameters, InterestRateCondition rate)
        {
            var rateKind = ((EnumMemberAttribute[])(typeof(InterestRateKinds)).GetField(rate.Kind.ToString())
                .GetCustomAttributes(typeof(EnumMemberAttribute), true))?.FirstOrDefault()?.Value;
            rate.Variations = rate.Variations ?? new List<InterestRateVariation>();
            rate.Variations = rate.Variations.Where(nv => nv.Origin != PriceVariationOrigins.ProductOptions).ToList();
            if (calculationParameters.Options != null && calculationParameters.Options.Count() > 0)
            {
                foreach (var option in calculationParameters.Options)
                {
                    var effectsForRate = option.Effects.Where(b => b.Kind == rateKind)?.ToList();
                    if (effectsForRate != null && effectsForRate.Count() > 0)
                    {
                        foreach (var effect in effectsForRate)
                        {
                            rate.Variations.Add(new InterestRateVariation
                            {
                                BenefitId = effect.Code,
                                BenefitSourceId = option.Code,
                                Origin = PriceVariationOrigins.ProductOptions,
                                Percentage = effect.Value,
                                VariationDescription = option.Description,
                                VariationGroup = effect.Description
                            });
                        }
                    }
                }
            }
            return rate;
        }

        private InterestRateCondition ResolveRateBenefits(PriceCalculationParameters calculationParameters, InterestRateCondition rate)
        {
            var rateKind = ((EnumMemberAttribute[])(typeof(InterestRateKinds)).GetField(rate.Kind.ToString())
                .GetCustomAttributes(typeof(EnumMemberAttribute), true))?.FirstOrDefault()?.Value;
            rate.Variations = rate.Variations ?? new List<InterestRateVariation>();
            rate.Variations = rate.Variations.Where(nv => nv.Origin != PriceVariationOrigins.Campaign).ToList();
            if (calculationParameters.Campaign?.Benefits != null)
            {
                var benefitsForRate = calculationParameters.Campaign.Benefits.Where(b => b.Kind == rateKind)?.ToList();
                if (benefitsForRate != null && benefitsForRate.Count() > 0)
                {
                    foreach (var benefit in benefitsForRate)
                    {
                        rate.Variations.Add(new InterestRateVariation
                        {
                            BenefitId = benefit.Code,
                            BenefitSourceId = calculationParameters.Campaign.CampaignCode,
                            Origin = PriceVariationOrigins.Campaign,
                            Percentage = benefit.Value,
                            VariationDescription = benefit.Description,
                            VariationGroup = calculationParameters.Campaign.Description
                        });
                    }
                }
            }
            return rate;
        }

        private decimal? GetInterestRateWithoutBundle(InterestRateCondition rate, string bundleGroupCode, DateTime? dateTime)
        {
            var tempRate = (InterestRateCondition)rate.Clone();
            tempRate.Variations = tempRate.Variations.Where(v => !bundleGroupCode.Equals(v.VariationGroup?.ToLower())).ToList();
            tempRate.LowerLimitVariations = tempRate.LowerLimitVariations.Where(v => !bundleGroupCode.Equals(v.VariationGroup)).ToList();
            tempRate.UpperLimitVariations = tempRate.UpperLimitVariations.Where(v => !bundleGroupCode.Equals(v.VariationGroup)).ToList();
            return GetPrice(tempRate, dateTime ?? DateTime.Today);
        }

        private async Task<ResolvedFeesResult> CalculateFees(PriceCalculationParameters calculationParameters, VariationDefinitionParams definitionParams)
        {
            // TODO Include all fee parameters (limits and percentage) in DMN and in calculation
            var result = new ResolvedFeesResult
            {
                ResultChanged = false
            };
            if (calculationParameters.Fees != null)
            {
                var arrFees = calculationParameters.Fees;
                result.Fees = new List<FeeCondition>();
                foreach (FeeCondition fee in arrFees)
                {
                    if (fee == null || fee.EffectiveDate > DateTime.Today ||
                        (fee.Currencies != null && !fee.Currencies.Contains(calculationParameters.Currency)))
                    {
                        continue;
                    }
                    var newFee = fee.Copy();
                    newFee = ResolveFeeBenefits(calculationParameters, newFee);
                    newFee = ResolveFeeOptions(calculationParameters, newFee);

                    if (!string.IsNullOrEmpty(newFee.VariationsDefinitionDMN))
                    {
                        var newVars = await priceCalculationService.ResolveFeeVariationDmn(newFee.VariationsDefinitionDMN, definitionParams, newFee.VariationsDefinition); ;
                        if (newVars != null && newVars.Count > 0)
                        {
                            if (newFee.Variations != null || newFee.Variations.Count > 0)
                            {
                                newFee.Variations = newFee.Variations.Where(nv => nv.Origin != PriceVariationOrigins.Product).ToList();
                            }
                            else
                            {
                                newFee.Variations = new List<FeeVariation>();
                            }

                            newFee.Variations.AddRange(newVars);
                        }
                    }

                    var calculatedFixedAmount = newFee.CalculatedFixedAmount;
                    var calculatedLowerLimit = newFee.CalculatedLowerLimit;
                    var calculatedUpperLimit = newFee.CalculatedUpperLimit;
                    var calculatedPercentage = newFee.CalculatedPercentage;
                    var calculatedFee = SolveFeeCondition(newFee);
                    if (calculatedFee.CalculatedPercentage != calculatedPercentage || calculatedFee.CalculatedFixedAmount != calculatedFixedAmount ||
                        calculatedFee.CalculatedLowerLimit != calculatedLowerLimit || calculatedFee.CalculatedUpperLimit != calculatedUpperLimit)
                    {
                        result.ResultChanged = true;
                    }
                    result.Fees.Add(calculatedFee);
                }
            }
            return result;
        }

        private FeeCondition ResolveFeeOptions(PriceCalculationParameters calculationParameters, FeeCondition fee)
        {
            var feeKind = ((EnumMemberAttribute[])(typeof(FeeConditionKind)).GetField(fee.Kind.ToString())
                .GetCustomAttributes(typeof(EnumMemberAttribute), true))?.FirstOrDefault()?.Value;
            fee.Variations = fee.Variations ?? new List<FeeVariation>();
            fee.Variations = fee.Variations.Where(nv => nv.Origin != PriceVariationOrigins.ProductOptions).ToList();
            if (calculationParameters.Options != null && calculationParameters.Options.Count() > 0)
            {
                foreach (var option in calculationParameters.Options)
                {
                    var effectsForFee = option.Effects.Where(b => b.Kind == feeKind)?.ToList();
                    if (effectsForFee != null && effectsForFee.Count() > 0)
                    {
                        foreach (var effect in effectsForFee)
                        {
                            fee.Variations.Add(new FeeVariation
                            {
                                BenefitId = effect.Code,
                                BenefitSourceId = option.Code,
                                Origin = PriceVariationOrigins.ProductOptions,
                                Percentage = effect.Value,
                                VariationDescription = option.Description,
                                VariationGroup = effect.Description
                            });
                        }
                    }
                }
            }
            return fee;
        }

        private FeeCondition ResolveFeeBenefits(PriceCalculationParameters calculationParameters, FeeCondition fee)
        {
            var feeKind = ((EnumMemberAttribute[])(typeof(FeeConditionKind)).GetField(fee.Kind.ToString())
                        .GetCustomAttributes(typeof(EnumMemberAttribute), true))?.FirstOrDefault()?.Value;
            fee.Variations = fee.Variations ?? new List<FeeVariation>();
            fee.Variations = fee.Variations.Where(nv => nv.Origin != PriceVariationOrigins.Campaign).ToList();
            if (calculationParameters.Campaign?.Benefits != null)
            {
                var benefitsForFee = calculationParameters.Campaign.Benefits.Where(b => b.Kind == feeKind)?.ToList();
                if (benefitsForFee != null && benefitsForFee.Count() > 0)
                {
                    foreach (var benefit in benefitsForFee)
                    {
                        fee.Variations.Add(new FeeVariation
                        {
                            BenefitId = benefit.Code,
                            BenefitSourceId = calculationParameters.Campaign.CampaignCode,
                            Origin = PriceVariationOrigins.Campaign,
                            Percentage = benefit.Value,
                            VariationDescription = benefit.Description,
                            VariationGroup = calculationParameters.Campaign.Description
                        });
                    }
                }
            }
            return fee;
        }

        private decimal? GetBaseRateValue(string baseRateId, DateTime date)
        {
            if (string.IsNullOrEmpty(baseRateId))
            {
                return null;
            }
            var referentRate = marketRatesService.GetRate(baseRateId, date).Result;
            return referentRate.Value;
        }
    }
}
