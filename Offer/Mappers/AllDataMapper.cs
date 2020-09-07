using AutoMapper;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using OfferApplication = Offer.Domain.AggregatesModel.ApplicationAggregate.Application;
using Offer.Domain.AggregatesModel.ApplicationAggregate.ApplicantModel;
using Offer.Domain.View.AllDataViews;
using System;
using System.Collections.Generic;
using System.Linq;
using PriceCalculation.Models.Pricing;
using InstallmentPlanApp = Offer.Domain.AggregatesModel.ApplicationAggregate.InstallmentPlanRow;
using Offer.Domain.AggregatesModel.ExposureModel;
using MicroserviceCommon.Services;
using MicroserviceCommon.Models;
using AssecoCurrencyConvertion;
using System.Globalization;

namespace Offer.API.Mappers
{
    public static class AllDataMapper
    {
        public static void Configure(IMapperConfigurationExpression cfg, IServiceProvider serviceProvider)
        {
            #region Party
            cfg.CreateMap<EmploymentData, EmploymentDataAllDataView>()
                .ForMember(dest => dest.ContinousWorkPeriod, opt => opt.Ignore())
                .ForMember(dest => dest.EmploymentCount, opt => opt.Ignore())
                .ForAllOtherMembers(o => o.Condition((source) => true));

            cfg.CreateMap<Party, PartyAllDataView>()
                .Include<IndividualParty, IndividualPartyAllDataView>()
                .Include<OrganizationParty, OrganizationPartyAllDataView>()
                .ForMember(dest => dest.RelationshipCount, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    #region Exposure
                    if (src.Application?.ExposureInfo != null)
                    {
                        var exposure = src.Application.ExposureInfo;
                        var riskCategory = ConfigurationRiskCategoryList(serviceProvider);
                        // CB exposure
                        var cbExposureForParty = new ExposureList
                        {
                            Exposures = exposure.CreditBureauExposure?.Exposures?
                                .Where(e => e.PartyId == src.CustomerNumber)
                                .ToList()
                        };
                        cbExposureForParty.TotalApprovedAmountInTargetCurrency = cbExposureForParty.
                            Exposures?.Where(x => riskCategory.Contains(x.RiskCategory)).Sum(e => e.ExposureApprovedInTargetCurrency) ?? 0;
                        cbExposureForParty.TotalOutstandingAmountInTargetCurrency = cbExposureForParty.
                            Exposures?.Where(x => riskCategory.Contains(x.RiskCategory)).Sum(e => e.ExposureOutstandingAmountInTargetCurrency) ?? 0;

                        // Current exposure
                        var currentExposureForParty = new ExposureList
                        {
                            Exposures = exposure.CurrentExposure?.Exposures?
                                .Where(e => e.PartyId == src.CustomerNumber)
                                .ToList()
                        };
                        currentExposureForParty.TotalApprovedAmountInTargetCurrency = currentExposureForParty.
                            Exposures?.Where(x => riskCategory.Contains(x.RiskCategory)).Sum(e => e.ExposureApprovedInTargetCurrency) ?? 0;
                        currentExposureForParty.TotalOutstandingAmountInTargetCurrency = currentExposureForParty.
                            Exposures?.Where(x => riskCategory.Contains(x.RiskCategory)).Sum(e => e.ExposureOutstandingAmountInTargetCurrency) ?? 0;

                        // new Expposure in other Apps
                        var newInOtherForParty = new ExposureList
                        {
                            Exposures = exposure.NewExposureInOtherApplications?.Exposures?
                                .Where(e => e.PartyId == src.CustomerNumber)
                                .ToList()
                        };
                        newInOtherForParty.TotalApprovedAmountInTargetCurrency = newInOtherForParty.
                            Exposures?.Where(x => riskCategory.Contains(x.RiskCategory)).Sum(e => e.ExposureApprovedInTargetCurrency) ?? 0;
                        newInOtherForParty.TotalOutstandingAmountInTargetCurrency = newInOtherForParty.
                            Exposures?.Where(x => riskCategory.Contains(x.RiskCategory)).Sum(e => e.ExposureOutstandingAmountInTargetCurrency) ?? 0;

                        // New Exposure in current App
                        var newInCurrentForParty = new ExposureList
                        {
                            Exposures = exposure.NewExposureInCurrentApplication?.Exposures?
                                .Where(e => e.PartyId == src.CustomerNumber)
                                .ToList()
                        };
                        newInCurrentForParty.TotalApprovedAmountInTargetCurrency = newInCurrentForParty.
                            Exposures?.Where(x => riskCategory.Contains(x.RiskCategory)).Sum(e => e.ExposureApprovedInTargetCurrency) ?? 0;
                        newInCurrentForParty.TotalOutstandingAmountInTargetCurrency = newInCurrentForParty.
                            Exposures?.Where(x => riskCategory.Contains(x.RiskCategory)).Sum(e => e.ExposureOutstandingAmountInTargetCurrency) ?? 0;

                        Dictionary<string, Currency> calc = new Dictionary<string, Currency>();
                        foreach (var item in exposure.Calculated)
                        {
                            string newKey = item.Key.Replace(' ', '-').ToLower();
                            calc.TryAdd(newKey, item.Value);

                        }

                        dest.Exposure = new ExposureInfo
                        {
                            CreditBureauExposure = cbExposureForParty,
                            CurrentExposure = currentExposureForParty,
                            NewExposureInOtherApplications = newInOtherForParty,
                            NewExposureInCurrentApplication = newInCurrentForParty,
                            TotalCbApprovedExposureAmount = cbExposureForParty.TotalApprovedAmountInTargetCurrency + newInOtherForParty.TotalApprovedAmountInTargetCurrency + newInCurrentForParty.TotalApprovedAmountInTargetCurrency,
                            TotalCbOutstandingAmount = cbExposureForParty.TotalOutstandingAmountInTargetCurrency + newInOtherForParty.TotalOutstandingAmountInTargetCurrency + newInCurrentForParty.TotalOutstandingAmountInTargetCurrency,
                            TotalExposureApprovedAmount = currentExposureForParty.TotalApprovedAmountInTargetCurrency + newInOtherForParty.TotalApprovedAmountInTargetCurrency + newInCurrentForParty.TotalApprovedAmountInTargetCurrency,
                            TotalExposureOutstandingAmount = currentExposureForParty.TotalOutstandingAmountInTargetCurrency + newInOtherForParty.TotalOutstandingAmountInTargetCurrency + newInCurrentForParty.TotalOutstandingAmountInTargetCurrency,
                            TotalExposureCurrency = exposure.TotalExposureCurrency,
                            Calculated = calc,
                            CurrencyExchangeRates = exposure.CurrencyExchangeRates
                        };
                    }
                    #endregion

                    if (src is IndividualParty individualParty && individualParty.FinancialProfile != null)
                    {
                        var destFinancialProfile = (dest as IndividualPartyAllDataView).AsIndividual.FinancialProfile;
                        destFinancialProfile = destFinancialProfile ?? new FinancialData
                        {
                            ExpenseInfo = new List<Expense>(),
                            IncomeInfo = new List<Income>()
                        };
                        destFinancialProfile.TotalExpenses = CalculateTotal(destFinancialProfile.ExpenseInfo, serviceProvider);
                        destFinancialProfile.TotalIncomes = CalculateTotal(destFinancialProfile.IncomeInfo, serviceProvider);
                    }
                })
                .ForAllOtherMembers(o => o.Condition((source) => true));

            cfg.CreateMap<IndividualParty, IndividualPartyAllDataView>()
                .AfterMap((src, dest) =>
                {
                    dest.AsIndividual = Mapper.Map<AsIndividualPartyView>(src);
                })
                .ForAllOtherMembers(o => o.Condition((source) => true));

            cfg.CreateMap<FinancialProfile, FinancialData>()
                .ForMember(dest => dest.TotalExpenses, opt => opt.MapAtRuntime())
                .ForMember(dest => dest.TotalIncomes, opt => opt.MapAtRuntime())
                .ForAllOtherMembers(o => o.Condition((source) => true));

            cfg.CreateMap<IndividualParty, AsIndividualPartyView>()
                .ForMember(dest => dest.Age, opt => opt.Ignore())
                .ForMember(dest => dest.AgeInMonths, opt => opt.Ignore())
                .ForMember(dest => dest.FinancialProfile, opt => opt.MapAtRuntime())
                .ForAllOtherMembers(o => o.Condition((source) => true));

            cfg.CreateMap<EmploymentData, EmploymentDataAllDataView>();

            cfg.CreateMap<OrganizationParty, OrganizationPartyAllDataView>()
                .AfterMap((src, dest) =>
                {
                    dest.AsOrganization = Mapper.Map<AsOrganizationPartyView>(src);
                });

            cfg.CreateMap<OrganizationParty, AsOrganizationPartyView>();
            #endregion

            #region Application
            cfg.CreateMap<OfferApplication, ApplicationAllDataView>()
                .ForMember(dest => dest.OrganizationUnit, opt => opt.MapFrom(obj => obj.OrganizationUnitCode))
                .ForMember(dest => dest.Channel, opt => opt.MapFrom(obj => obj.ChannelCode))
                .ForMember(dest => dest.InvolvedPartyCount, opt => opt.Ignore())
                .ForAllOtherMembers(o => o.Condition((source) => true));
            #endregion

            #region ArrangementRequest
            cfg.CreateMap<ArrangementRequest, ArrangementRequestAllDataView>()
                .Include<CardAccessArrangementRequest, CardAccessArrangementRequestAllDataView>()
                .Include<CreditFacilityRequest, CreditFacilityRequestAllDataView>()
                .Include<CreditCardFacilityRequest, CreditCardFacilityRequestAllDataView>()
                .Include<CurrentAccountRequest, CurrentAccountRequestAllDataView>()
                .Include<DemandDepositRequest, DemandDepositRequestAllDataView>()
                .Include<ElectronicAccessArrangementRequest, ElectronicAccessArrangementRequestAllDataView>()
                .Include<OverdraftFacilityRequest, OverdraftFacilityRequestAllDataView>()
                .Include<TermDepositRequest, TermDepositRequestAllDataView>()
                .Include<TermLoanRequest, TermLoanRequestAllDataRequest>()
                .Include<SecuritiesArrangementRequest, SecuritiesArrangementRequestAllDataView>()
                .Include<OtherProductArrangementRequest, OtherProductArrangementRequestAllDataView>()
                .Include<AbstractArrangementRequest, AbstractArrangementRequestAllDataView>()
                .Include<CreditLineRequest, CreditLineRequestAllDataView>()
                .ForMember(dest => dest.ProductFamily, opt => opt.MapFrom(obj => obj.ProductSnapshot.FamilyName))
                .AfterMap((src, dest) =>
                {
                    if (!src.ArrangementKind.HasValue || !(new List<ArrangementKind> { ArrangementKind.CreditCardFacility, ArrangementKind.OverdraftFacility, ArrangementKind.TermLoan }).Contains(src.ArrangementKind.Value))
                    {
                        return;
                    }
                    dest.Calculation = new CalculationAllDataView
                    {
                        CalculationDate = src.CalculationDate,
                        InstallmentPlan = src.InstallmentPlan,
                        NumberOfInstallments = src.NumberOfInstallments,
                        OverrideProductLimits = src.OverrideProductLimits,
                        Periods = src.Periods,
                        TotalDisbursement = 0,
                        TotalAnnuity = 0,
                        TotalCashCollateral = 0,
                        TotalDiscountedNetCashFlow = 0,
                        TotalExpenses = 0,
                        TotalExpensesInDomesticCurrency = 0,
                        TotalFeeAmount = 0,
                        TotalInterest = 0,
                        TotalNetCashFlow = 0,
                        TotalPrincipal = 0,
                        TotalRepaymentAmount = 0
                    };
                    foreach (InstallmentPlanApp row in src.InstallmentPlan)
                    {
                        dest.Calculation.TotalDisbursement += row.Disbursement;
                        dest.Calculation.TotalAnnuity += row.Annuity;
                        dest.Calculation.TotalPrincipal += row.PrincipalRepayment;
                        dest.Calculation.TotalInterest += row.InterestRepayment;
                        dest.Calculation.TotalExpenses += (row.Fee + row.OtherExpenses);
                        dest.Calculation.TotalCashCollateral += row.CashCollateral;
                        dest.Calculation.TotalNetCashFlow += row.NetCashFlow;
                        dest.Calculation.TotalDiscountedNetCashFlow += row.DiscountedNetCashFlow;
                        dest.Calculation.TotalRepaymentAmount += (row.Fee + row.InterestRepayment + row.PrincipalRepayment + row.OtherExpenses);
                    }

                    if (dest.Conditions?.MainInterestRate != null && dest.Conditions.MainInterestRate.IsFixed)
                    {
                        dest.Calculation.InterestRateVariability = "fixed";
                    }
                    else
                    {
                        dest.Calculation.InterestRateVariability = "variable";
                    }

                    if (dest.Conditions?.MainInterestRate != null && dest.Conditions.MainInterestRate.IsCompound)
                    {
                        dest.Calculation.CalculationMethod = "compound";
                    }
                    else
                    {
                        dest.Calculation.CalculationMethod = "simple";
                    }
                });

            cfg.CreateMap<CardAccessArrangementRequest, CardAccessArrangementRequestAllDataView>();
            cfg.CreateMap<CreditFacilityRequest, CreditFacilityRequestAllDataView>();
            cfg.CreateMap<CreditCardFacilityRequest, CreditCardFacilityRequestAllDataView>();
            cfg.CreateMap<CurrentAccountRequest, CurrentAccountRequestAllDataView>();
            cfg.CreateMap<DemandDepositRequest, DemandDepositRequestAllDataView>();
            cfg.CreateMap<ElectronicAccessArrangementRequest, ElectronicAccessArrangementRequestAllDataView>();
            cfg.CreateMap<OverdraftFacilityRequest, OverdraftFacilityRequestAllDataView>();
            cfg.CreateMap<TermDepositRequest, TermDepositRequestAllDataView>();
            cfg.CreateMap<TermLoanRequest, TermLoanRequestAllDataRequest>()
                .ForMember(dest => dest.FinanceServiceDetails, opt => opt.MapFrom(obj => new FinanceServiceDetails
                {
                    Amount = obj.Amount,
                    Napr = obj.Napr,
                    Eapr = obj.Eapr,
                    Term = obj.Term,
                    Annuity = obj.Annuity,
                    DownpaymentAmount = obj.DownpaymentAmount,
                    DownpaymentPercentage = obj.DownpaymentPercentage,
                    InvoiceAmount = obj.InvoiceAmount,
                    AmountInDomesticCurrency = obj.AmountInDomesticCurrency
                }))
                .ForMember(dest => dest.CurrentValues, opt => opt.MapFrom(obj => new CurrentValues
                {
                    Amount = obj.Amount,
                    Annuity = obj.Annuity,
                    DownpaymentAmount = obj.DownpaymentAmount,
                    DownpaymentPercentage = obj.DownpaymentPercentage,
                    InvoiceAmount = obj.InvoiceAmount,
                    Napr = obj.Napr,
                    Eapr = obj.Eapr,
                    Term = obj.Term,
                    AmountInDomesticCurrency = obj.AmountInDomesticCurrency
                }));
            cfg.CreateMap<SecuritiesArrangementRequest, SecuritiesArrangementRequestAllDataView>();
            cfg.CreateMap<OtherProductArrangementRequest, OtherProductArrangementRequestAllDataView>();
            cfg.CreateMap<AbstractArrangementRequest, AbstractArrangementRequestAllDataView>();
            cfg.CreateMap<CreditLineRequest, CreditLineRequestAllDataView>();
            cfg.CreateMap<FinanceServiceArrangementRequest, FinanceServiceRequestAllDataView>();

            cfg.CreateMap<Conditions, ConditionsAllDataView>();
            cfg.CreateMap<PricingCondition, PricingConditionAllDataView>()
                .Include<FeeCondition, FeeConditionAllDataView>()
                .Include<InterestRateCondition, InterestRateConditionAllDataView>();
            cfg.CreateMap<FeeCondition, FeeConditionAllDataView>();
            cfg.CreateMap<InterestRateCondition, InterestRateConditionAllDataView>()
                .AfterMap((src, dest) =>
                {
                    dest.IsMain = string.IsNullOrEmpty(src.Periods);
                });

            cfg.CreateMap<PricingCondition, PricingConditionAllDataView>();
            #endregion

        }

        private static Currency CalculateTotal(IReadOnlyList<BaseAmount> amountList, IServiceProvider serviceProvider)
        {
            IConfigurationService configService = (IConfigurationService)serviceProvider.GetService(typeof(IConfigurationService));
            // Convert amounts to TargetCurrency and return total
            var targetCurrency = configService.GetEffective("offer/exposure/target-currency", "EUR").Result;
            var conversionMethod = configService.GetEffective("offer/exposure/currency-conversion-method", "Buy to middle").Result;
            Currency total = new Currency { Amount = 0, Code = targetCurrency };
            if (amountList != null)
            {
                foreach (var member in amountList)
                {
                    decimal convertedAmount = member.Amount.Amount;
                    if (!targetCurrency.Equals(member.Amount.Code))
                    {
                        convertedAmount = new CurrencyConverter().CurrencyConvert(member.Amount.Amount, member.Amount.Code, targetCurrency, DateTime.Today.ToString("o", CultureInfo.InvariantCulture), conversionMethod);
                    }
                    total.Amount += convertedAmount;
                }
            }
            return total;
        }

        private static List<string> ConfigurationRiskCategoryList(IServiceProvider serviceProvider)
        {
            IConfigurationService configService = (IConfigurationService)serviceProvider.GetService(typeof(IConfigurationService));
            var riskCategory = configService.GetEffective("offer/exposure/risk-category", "1").Result.
                            Split(",").Where(c => !string.IsNullOrWhiteSpace(c)).Select(p => p.Trim()).ToList();
            return riskCategory;
        }
    }
}
