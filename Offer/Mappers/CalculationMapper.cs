using AutoMapper;
using CalculateInstallmentPlanRequestApp = CalculationService.Models.CalculateInstallmentPlanRequest;
using CalculationService.Services;
using Offer.API.Application.Commands;
using Offer.Domain.AggregatesModel.Calculations;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using static Offer.Domain.Calculations.InstallmentPlanCalculation;
using System.Linq;
using Offer.Domain.Calculations;
using System.Collections.Generic;
using Offer.Domain.Utils;
using PriceCalculation.Calculations;

namespace Offer.API.Mappers
{
    public static class CalculationMapper
    {
        public static void Configure(IMapperConfigurationExpression cfg)
        {
            /*cfg.CreateMap<CalculationService.Models.InstallmentPlanRow, Domain.AggregatesModel.ApplicationAggregate.InstallmentPlanRow>()
                .ForMember(dest => dest.ActivityKind, opt => opt.MapFrom(o => Enum.Parse<ActivityKind>(o.ActivityKind.Replace("stdloan-", ""))))
                .ForMember(dest => dest.YearFrac, opt => opt.MapFrom(o => 0));*/
            cfg.CreateMap<InitiateCalculateOfferCommand, ResolveSchedulingPeriodsRequest>();
            cfg.CreateMap<CalculateInstallmentPlanRequestApp, KdpInstallmentPlanCalculationRequest>()
                .ForMember(dest => dest.ArrangementType, opt => opt.MapFrom(obj => obj.ArrangementType))
                .ForMember(dest => dest.AccountNumber, opt => opt.MapFrom(obj => obj.AccountNumber ?? ""))
                .ForMember(dest => dest.DealWithHisotry, opt => opt.MapFrom(obj => obj.DealWithHisotry))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(obj => obj.Amount))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(obj => obj.StartDate))
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(obj => obj.Currency ?? "978"))
                .ForMember(dest => dest.DrawdownPeriodEnd, opt => opt.MapFrom(obj => obj.DrawdownPeriodEnd == null ? obj.StartDate : obj.DrawdownPeriodEnd))
                .ForMember(dest => dest.GracePeriodEnd, opt => opt.MapFrom(obj => obj.GracePeriodEnd == null ? obj.StartDate : obj.GracePeriodEnd))
                .ForMember(dest => dest.InterestCalculationFrequencyPeriod, opt => opt.MapFrom(obj => obj.InterestCalculationFrequencyPeriod))
                .ForMember(dest => dest.InterestCalculationFrequencyUnitOfTime, opt => opt.MapFrom(obj => obj.InterestCalculationFrequencyUnitOfTime))
                .ForMember(dest => dest.InterestAlwaysWithInstallment, opt => opt.MapFrom(obj => obj.InterestAlwaysWithInstallment))
                .ForMember(dest => dest.CustomPrincipalFlowOption, opt => opt.MapFrom(obj => obj.CustomPrincipalFlowOption))
                .ForMember(dest => dest.StartingBalanceDate, opt => opt.MapFrom(obj => obj.StartingBalanceDate == null ? obj.StartDate : obj.StartingBalanceDate))
                .ForMember(dest => dest.FirstInterestDateInDrawdownPeriod, opt => opt.MapFrom(obj => obj.FirstInterestDateInDrawdownPeriod == null ? obj.StartDate : obj.FirstInterestDateInDrawdownPeriod))
                .ForMember(dest => dest.FirstManagementFeeDate, opt => opt.MapFrom(obj => obj.FirstManagementFeeDate == null ? obj.StartDate : obj.FirstManagementFeeDate))
                .ForMember(dest => dest.ManagementFeeCalculationFrequencyPeriod, opt => opt.MapFrom(obj => obj.ManagementFeeCalculationFrequencyPeriod))
                .ForMember(dest => dest.ManagementFeeCalculationFrequencyUnitOfTime, opt => opt.MapFrom(obj => obj.ManagementFeeCalculationFrequencyUnitOfTime))
                .ForMember(dest => dest.FirstInterestDateInGrace, opt => opt.MapFrom(obj => obj.FirstInterestDateInGrace == null ? obj.StartDate : obj.FirstInterestDateInGrace))
                .ForMember(dest => dest.FirstInterestDateInGraceFollowTheEndOfMonth, opt => opt.MapFrom(obj => obj.FirstInterestDateInGraceFollowTheEndOfMonth))
                .ForMember(dest => dest.InterestCalculationFrequencyInGracePeriod, opt => opt.MapFrom(obj => obj.InterestCalculationFrequencyInGracePeriod))
                .ForMember(dest => dest.InterestCalculationFrequencyInGraceUnitOfTime, opt => opt.MapFrom(obj => obj.InterestCalculationFrequencyInGraceUnitOfTime))
                .ForMember(dest => dest.FirstInterestDateInRepaymentPeriod, opt => opt.MapFrom(obj => obj.FirstInterestDateInRepaymentPeriod == null ? obj.StartDate.Value.AddMonths(1) : obj.FirstInterestDateInRepaymentPeriod))
                .ForMember(dest => dest.FirstInterestDateInRepaymentFollowTheEndOfMonth, opt => opt.MapFrom(obj => obj.FirstInterestDateInRepaymentFollowTheEndOfMonth))
                .ForMember(dest => dest.InterestCalculationMethod, opt => opt.MapFrom(obj => obj.InterestCalculationMethod))
                .ForMember(dest => dest.FirstInstallmentDate, opt => opt.MapFrom(obj => obj.FirstInstallmentDate == null ? obj.StartDate.Value.AddMonths(1) : obj.FirstInstallmentDate))
                .ForMember(dest => dest.InstallmentFrequencyPeriod, opt => opt.MapFrom(obj => obj.InstallmentFrequencyPeriod))
                .ForMember(dest => dest.InstallmentFrequencyUnitOfTime, opt => opt.MapFrom(obj => obj.InstallmentFrequencyUnitOfTime))
                .ForMember(dest => dest.FollowTheEndOfMonth, opt => opt.MapFrom(obj => obj.FollowTheEndOfMonth))
                .ForMember(dest => dest.RepaymentType, opt => opt.MapFrom(obj => obj.RepaymentType))
                .ForMember(dest => dest.ConditionContainer, opt => opt.MapFrom(obj => obj.ConditionContainer))
                .ForMember(dest => dest.TrancheNumber, opt => opt.MapFrom(obj => obj.TrancheNumber))
                .ForMember(dest => dest.RegularInterestPercentage, opt => opt.MapFrom(obj => obj.RegularInterestPercentage))
                .ForMember(dest => dest.RegularInterestUnitOfTime, opt => opt.MapFrom(obj => obj.RegularInterestUnitOfTime))
                .ForMember(dest => dest.OriginationFeeCapitalization, opt => opt.MapFrom(obj => obj.OriginationFeeCapitalization))
                .ForMember(dest => dest.OriginationFeePercentage, opt => opt.MapFrom(obj => obj.OriginationFeePercentage))
                .ForMember(dest => dest.OriginationFeeFixedAmountLcl, opt => opt.MapFrom(obj => obj.OriginationFeeFixedAmountLcl))
                .ForMember(dest => dest.OriginationFeeLowerLimit, opt => opt.MapFrom(obj => obj.OriginationFeeLowerLimit))
                .ForMember(dest => dest.OriginationFeeLowerLimitLclValAmount, opt => opt.MapFrom(obj => obj.OriginationFeeLowerLimitLclValAmount))
                .ForMember(dest => dest.OriginationFeeUpperLimit, opt => opt.MapFrom(obj => obj.OriginationFeeUpperLimit))
                .ForMember(dest => dest.OriginationFeeUpperLimitLclValAmount, opt => opt.MapFrom(obj => obj.OriginationFeeUpperLimitLclValAmount))
                .ForMember(dest => dest.ManagementFeePercentage, opt => opt.MapFrom(obj => obj.ManagementFeePercentage))
                .ForMember(dest => dest.ManagementFeeLowerLimit, opt => opt.MapFrom(obj => obj.ManagementFeeLowerLimit))
                .ForMember(dest => dest.CustomerIdentifier, opt => opt.MapFrom(obj => obj.CustomerIdentifier))
                .ForMember(dest => dest.Product, opt => opt.MapFrom(obj => obj.Product))
                .ForMember(dest => dest.FixedAnnuity, opt => opt.MapFrom(obj => obj.FixedAnnuity != 0))
                .ForMember(dest => dest.FixedAnnuityAmount, opt => opt.MapFrom(obj => obj.FixedAnnuity != 0 ? obj.FixedAnnuity : 0))
                .ForMember(dest => dest.DealWithHisotrySpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.AmountSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.StartDateSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.DrawdownPeriodEndSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.GracePeriodEndSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.InterestCalculationFrequencyPeriodSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.InterestCalculationFrequencyUnitOfTimeSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.InterestAlwaysWithInstallmentSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.CustomPrincipalFlowOptionSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.StartingBalanceDateSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.FirstInterestDateInDrawdownPeriodSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.FirstManagementFeeDateSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.ManagementFeeCalculationFrequencyPeriodSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.ManagementFeeCalculationFrequencyUnitOfTimeSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.FirstInterestDateInGraceSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.FirstInterestDateInGraceFollowTheEndOfMonthSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.InterestCalculationFrequencyInGracePeriodSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.InterestCalculationFrequencyInGraceUnitOfTimeSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.FirstInterestDateInRepaymentPeriodSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.FirstInterestDateInRepaymentFollowTheEndOfMonthSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.FirstInstallmentDateSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.InstallmentFrequencyPeriodSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.InstallmentFrequencyUnitOfTimeSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.FollowTheEndOfMonthSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.RepaymentTypeSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.RegularInterestPercentageSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.RegularInterestUnitOfTimeSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.OriginationFeePercentageSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.OriginationFeeFixedAmountLclSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.OriginationFeeLowerLimitSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.OriginationFeeLowerLimitLclValAmountSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.OriginationFeeUpperLimitSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.OriginationFeeUpperLimitLclValAmountSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.OriginationFeeCapitalization, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.ManagementFeePercentageSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.ManagementFeeLowerLimitSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.FixedAnnuitySpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.FixedAnnuityAmountSpecified, opt => opt.MapFrom(obj => true))
                .ForMember(dest => dest.RegularInterestPercentageSpecified, opt => opt.MapFrom(obj => true))
                .ForAllOtherMembers(d => d.Ignore());
            cfg.CreateMap<InitiateCalculateOfferCommand, SimpleLoanCalculationRequest>()
               .ForMember(dest => dest.RegularInterest, opt => opt.MapFrom(src => src.ProductConditions.InterestRates.Where(i => i.Currencies.Contains(src.Currency)).Select(x => new InterestRateEntry
               {
                   RatePercentage = (double)x.CalculatedRate,
                   IsCompound = x.IsCompound,
                   CalendarBasis = x.CalendarBasis,
                   Name = x.Title,
                   RateUnitOfTime = Domain.Calculations.SimpleUnitOfTime.Y
               })))
               .ForMember(dest => dest.Fees, opt => opt.MapAtRuntime())
               .ForMember(dest => dest.StardDate, opt => opt.MapFrom(obj => obj.CalculationDate))
               .AfterMap((src, dest) =>
               {
                   dest.Fees = new List<FeeEntry>();
                   src.ProductConditions.Fees.ForEach(x =>
                   {
                       dest.Fees.Add(new FeeEntry
                       {
                           Kind = x.Kind,
                           Name = x.Title,
                           Frequency = x.Frequency,
                           Percentage = x.Percentage,
                           Currency = x.FixedAmount?.Code ?? "",
                           FixedAmount = x.FixedAmount?.Amount ?? 0,
                           LowerLimit = x.LowerLimit?.Amount ?? 0,
                           UpperLimit = x.UpperLimit?.Amount ?? 0,
                           ServiceCode = x.ServiceCode,
                           Date = dest.StardDate,
                           CalculationBasisType = CalculationBasisType.AccountBalance
                       });
                   });
               });
            cfg.CreateMap<ArrangementRequestInitializationParameters, SimpleLoanCalculationRequest>()
               .ForMember(dest => dest.RegularInterest, opt => opt.MapFrom(src => src.Conditions.InterestRates.Where(i => i.Currencies.Contains(src.Currency)).Select(x => new InterestRateEntry
               {
                   RatePercentage = (double)x.CalculatedRate,
                   IsCompound = x.IsCompound,
                   CalendarBasis = x.CalendarBasis,
                   Name = x.Title,
                   RateUnitOfTime = Domain.Calculations.SimpleUnitOfTime.Y
               })))
               .ForMember(dest => dest.Fees, opt => opt.MapAtRuntime())
               .ForMember(dest => dest.StardDate, opt => opt.MapFrom(obj => obj.CalculationDate))
               .AfterMap((src, dest) =>
               {
                   dest.Fees = new List<FeeEntry>();
                   src.Conditions.Fees.ForEach(x =>
                   {
                       dest.Fees.Add(new FeeEntry
                       {
                           Kind = x.Kind,
                           Name = x.Title,
                           Frequency = x.Frequency,
                           Percentage = x.Percentage,
                           Currency = x.FixedAmount?.Code ?? "",
                           FixedAmount = x.FixedAmount?.Amount ?? 0,
                           LowerLimit = x.LowerLimit?.Amount ?? 0,
                           UpperLimit = x.UpperLimit?.Amount ?? 0,
                           ServiceCode = x.ServiceCode,
                           Date = dest.StardDate,
                           CalculationBasisType = CalculationBasisType.AccountBalance
                       });
                   });
               });
            cfg.CreateMap<ArrangementRequest, SimpleLoanCalculationRequest>()
                .Include<TermLoanRequest, SimpleLoanCalculationRequest>()
                .Include<OverdraftFacilityRequest, SimpleLoanCalculationRequest>()
                .Include<CreditCardFacilityRequest, SimpleLoanCalculationRequest>();
            int numberOfInstallments;
            cfg.CreateMap<TermLoanRequest, SimpleLoanCalculationRequest>()
                .ForMember(dest => dest.NumberOfInstallments, opt => opt.MapAtRuntime())
                .AfterMap((src, dest) =>
                {
                    if (!string.IsNullOrEmpty(src.Term) && src.Term.StartsWith("P"))
                    {
                        dest.NumberOfInstallments = Utility.GetMonthsFromPeriod(src.Term);
                    }
                    else
                    {
                        dest.NumberOfInstallments = int.TryParse(src.Term, out numberOfInstallments) ? numberOfInstallments : 0;
                    }
                })
                .ForMember(dest => dest.InstallmentSchedule, opt => opt.MapFrom(src => new SimpleSchedule
                {
                    DayOfMonth = src.InstallmentScheduleDayOfMonth
                }))
                .ForMember(dest => dest.Fees, opt => opt.Ignore())
                .ForMember(dest => dest.RegularInterest, opt => opt.Ignore())
                .ForMember(dest => dest.MinimumDaysForFirstInstallment, opt => opt.MapFrom(obj => obj.ProductSnapshot.MinimumDaysForFirstInstallment))
                .ForMember(dest => dest.StardDate, opt => opt.MapFrom(obj => obj.CalculationDate));
            cfg.CreateMap<OverdraftFacilityRequest, SimpleLoanCalculationRequest>()
                .ForMember(dest => dest.NumberOfInstallments, opt => opt.MapAtRuntime())
                .AfterMap((src, dest) =>
                {
                    if (!string.IsNullOrEmpty(src.Term) && src.Term.StartsWith("P"))
                    {
                        dest.NumberOfInstallments = Utility.GetMonthsFromPeriod(src.Term);
                    }
                    else
                    {
                        dest.NumberOfInstallments = int.TryParse(src.Term, out numberOfInstallments) ? numberOfInstallments : 0;
                    }
                })
                .ForMember(dest => dest.InstallmentSchedule, opt => opt.MapFrom(src => new SimpleSchedule
                {
                    DayOfMonth = src.InstallmentScheduleDayOfMonth
                }))
                .ForMember(dest => dest.Fees, opt => opt.Ignore())
                .ForMember(dest => dest.RegularInterest, opt => opt.Ignore())
                .ForMember(dest => dest.MinimumDaysForFirstInstallment, opt => opt.MapFrom(obj => obj.ProductSnapshot.MinimumDaysForFirstInstallment))
                .ForMember(dest => dest.StardDate, opt => opt.MapFrom(obj => obj.CalculationDate));
            cfg.CreateMap<CreditCardFacilityRequest, SimpleLoanCalculationRequest>()
                .ForMember(dest => dest.NumberOfInstallments, opt => opt.MapAtRuntime())
                .AfterMap((src, dest) =>
                {
                    if (!string.IsNullOrEmpty(src.Term) && src.Term.StartsWith("P"))
                    {
                        dest.NumberOfInstallments = Utility.GetMonthsFromPeriod(src.Term);
                    }
                    else
                    {
                        dest.NumberOfInstallments = int.TryParse(src.Term, out numberOfInstallments) ? numberOfInstallments : 0;
                    }
                })
                .ForMember(dest => dest.InstallmentSchedule, opt => opt.MapFrom(src => new SimpleSchedule
                {
                    DayOfMonth = src.InstallmentScheduleDayOfMonth
                }))
                .ForMember(dest => dest.Fees, opt => opt.Ignore())
                .ForMember(dest => dest.RegularInterest, opt => opt.Ignore())
                .ForMember(dest => dest.MinimumDaysForFirstInstallment, opt => opt.MapFrom(obj => obj.ProductSnapshot.MinimumDaysForFirstInstallment))
                .ForMember(dest => dest.StardDate, opt => opt.MapFrom(obj => obj.CalculationDate));
        }
    }
}
