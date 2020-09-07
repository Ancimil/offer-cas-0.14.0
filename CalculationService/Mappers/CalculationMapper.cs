using AutoMapper;
using CalculationService.Models;
using CalculationService.Services;
using System;

namespace CalculationService.Mappers
{
    public static class CalculationMapper
    {
        public static void Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<CalculateInstallmentPlanRequest, KdpInstallmentPlanCalculationRequest>()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(obj => obj.StartDate ?? DateTime.Today))
                .ForMember(dest => dest.StartingBalanceDate, opt => opt.MapFrom(obj => obj.StartingBalanceDate ?? DateTime.Today))
                .ForMember(dest => dest.StartingBalanceDate, opt => opt.MapFrom(obj => obj.StartDate ?? DateTime.Today))
                .ForMember(dest => dest.FirstInterestDateInDrawdownPeriod, opt => opt.MapFrom(obj => obj.StartDate ?? DateTime.Today))
                .ForMember(dest => dest.FirstManagementFeeDate, opt => opt.MapFrom(obj => obj.StartDate ?? DateTime.Today))
                .ForMember(dest => dest.FirstInterestDateInGrace, opt => opt.MapFrom(obj => obj.StartDate ?? DateTime.Today))
                .ForMember(dest => dest.FirstInterestDateInGrace, opt => opt.MapFrom(obj => (obj.StartDate ?? DateTime.Today).AddMonths(1)))
                .ForMember(dest => dest.RegularInterestPercentage, opt => opt.MapFrom(obj => obj.InterestRate.GetValueOrDefault()))
                .ForMember(dest => dest.OriginationFeePercentage, opt => opt.MapFrom(obj => obj.OriginationFeePercentage.GetValueOrDefault()))
                .ForMember(dest => dest.OriginationFeeFixedAmountLcl, opt => opt.MapFrom(obj => obj.OriginationFeeFixedAmount.GetValueOrDefault()))
                .ForMember(dest => dest.FixedAnnuity, opt => opt.MapFrom(obj => obj.FixedAnnuity != 0))
                .ForMember(dest => dest.FixedAnnuityAmount, opt => opt.MapFrom(obj => obj.FixedAnnuity));
        }
    }
}
