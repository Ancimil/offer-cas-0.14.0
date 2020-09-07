using AutoMapper;
using Offer.API.Application.Commands;
using Offer.API.Application.Commands.ArrangementRequests;
using Offer.API.Application.Commands.ArrangementRequests.Collateral;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.AggregatesModel.ApplicationAggregate.ApplicantModel;
using PriceCalculation.Models.Pricing;
using Offer.Domain.Utils;
using Offer.Domain.View;
using Offer.Infrastructure.View;
using System;
using OfferApplication = Offer.Domain.AggregatesModel.ApplicationAggregate.Application;

namespace Offer.API.Mappers
{
    public class ConfigureMapper
    {
        public static void Configure(IServiceProvider serviceProvider)
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<ProductData, ProductSnapshot>().ForMember(
                    dest => dest.PrimaryCurrency,
                    opt => opt.MapFrom(src => src.PrimaryCurrency)
                );
                cfg.CreateMap<ArrangementRequest, PriceCalculationResponse>();
                cfg.CreateMap<CreateApplicationDocumentCommand, ApplicationDocument>()
                .ForMember(
                    dest => dest.ApplicationId,
                    opt => opt.MapFrom(src => src.ApplicationNumber)
                )
                .ForAllMembers(o => o.Condition((source, destination, srcMember) => true));
                cfg.CreateMap<Offer.Domain.AggregatesModel.ApplicationAggregate.Application, ApplicationView>(MemberList.Source);
                cfg.CreateMap<UpdateCollateralRequirementCommand, CollateralRequirement>();
                cfg.CreateMap<IndividualParty, GdprParty>()
                .ForMember(
                    dest => dest.ApplicationStatus,
                    opt => opt.MapFrom(src => src.Application.Status)
                );
                cfg.CreateMap<Questionnaire, GdprQuestionnaire>();
                cfg.CreateMap<ProductDocumentation, ApplicationDocument>()
                .ForMember(
                    dest => dest.DocumentKind,
                    opt => opt.MapFrom(src => src.DocumentType)
                )
                .ForAllOtherMembers(o => o.Condition((source, destination, srcMember) => srcMember != null));
                cfg.CreateMap<ArrangementRequestValidation, ArrangementRequestValidationData>()
                .ForAllMembers(o => o.Condition((source, destination, srcMember) => srcMember != null));

                cfg.CreateMap<ApplicationDocument, ApplicationDocumentView>();
                cfg.CreateMap<OfferApplication, ApplicationDetailsView>();
                cfg.CreateMap<Party, DataCompletenessResponse>();
                // InvolvedParty Related Mappers
                ConfigureInvolvedPartyRelated(cfg);
                ConfigureRequestInitParams(cfg);
                AllDataMapper.Configure(cfg, serviceProvider);
            });
        }

        public static void ConfigureInvolvedPartyRelated(IMapperConfigurationExpression cfg)
        {
            IndividualMapper.Configure(cfg);
            OrganizationMapper.Configure(cfg);
            CalculationMapper.Configure(cfg);
            cfg.CreateMap<IndividualParty, IndividualGeneralInformation>()
                .ReverseMap()
                .ForAllMembers(o => o.Condition((source, destination, srcMember) => srcMember != null));
            cfg.CreateMap<OrganizationParty, OrganizationGeneralInformation>()
            .ReverseMap()
            .ForAllMembers(o => o.Condition((source, destination, srcMember) => srcMember != null));
            cfg.CreateMap<UpdateEmploymentInfoCommand, EmploymentData>();
            cfg.CreateMap<UpdateHouseholdInfoCommand, Household>();
            cfg.CreateMap<IndividualParty, IndividualContactPoints>()
                .ReverseMap()
                .ForAllMembers(o => o.Condition((source, destination, srcMember) => srcMember != null));
            cfg.CreateMap<IndividualParty, ContactPoints>()
                .ReverseMap()
                .ForAllMembers(o => o.Condition((source, destination, srcMember) => srcMember != null));
            cfg.CreateMap<OrganizationParty, ContactPoints>()
                .ReverseMap()
                .ForAllMembers(o => o.Condition((source, destination, srcMember) => srcMember != null));
            cfg.CreateMap<UpdateFinancialProfileCommand, FinancialProfile>();
        }

        public static void ConfigureRequestInitParams(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<InitiateOnlineOfferCommand, ArrangementRequestInitializationParameters>();
            cfg.CreateMap<InitiateCalculateOfferCommand, ArrangementRequestInitializationParameters>();
            cfg.CreateMap<ArrangementRequest, ArrangementRequestInitializationParameters>()
                .IncludeAllDerived();
            cfg.CreateMap<CardAccessArrangementRequest, ArrangementRequestInitializationParameters>();
            cfg.CreateMap<CreditFacilityRequest, ArrangementRequestInitializationParameters>();
            cfg.CreateMap<CreditCardFacilityRequest, ArrangementRequestInitializationParameters>();
            cfg.CreateMap<CurrentAccountRequest, ArrangementRequestInitializationParameters>();
            cfg.CreateMap<DemandDepositRequest, ArrangementRequestInitializationParameters>();
            cfg.CreateMap<ElectronicAccessArrangementRequest, ArrangementRequestInitializationParameters>();
            cfg.CreateMap<OverdraftFacilityRequest, ArrangementRequestInitializationParameters>();
            cfg.CreateMap<TermDepositRequest, ArrangementRequestInitializationParameters>();
            cfg.CreateMap<TermLoanRequest, ArrangementRequestInitializationParameters>();
            cfg.CreateMap<SecuritiesArrangementRequest, ArrangementRequestInitializationParameters>();
            cfg.CreateMap<OtherProductArrangementRequest, ArrangementRequestInitializationParameters>();
            cfg.CreateMap<AbstractArrangementRequest, ArrangementRequestInitializationParameters>();
            cfg.CreateMap<CreditLineRequest, ArrangementRequestInitializationParameters>();

            /*cfg.CreateMap<ArrangementRequest, ArrangementRequest>()
                .IncludeAllDerived();
            cfg.CreateMap<CardAccessArrangementRequest, ArrangementRequest>();
            cfg.CreateMap<CreditFacilityRequest, ArrangementRequest>();
            cfg.CreateMap<CreditCardFacilityRequest, ArrangementRequest>();
            cfg.CreateMap<CurrentAccountRequest, ArrangementRequest>();
            cfg.CreateMap<DemandDepositRequest, ArrangementRequest>();
            cfg.CreateMap<ElectronicAccessArrangementRequest, ArrangementRequest>();
            cfg.CreateMap<OverdraftFacilityRequest, ArrangementRequest>();
            cfg.CreateMap<TermDepositRequest, ArrangementRequest>();
            cfg.CreateMap<TermLoanRequest, ArrangementRequest>();
            cfg.CreateMap<SecuritiesArrangementRequest, ArrangementRequest>();
            cfg.CreateMap<OtherProductArrangementRequest, ArrangementRequest>();
            cfg.CreateMap<AbstractArrangementRequest, ArrangementRequest>();*/
        }
    }

    public static class MappingExpressionExtensions
    {
        public static IMappingExpression<TSource, TDest> IgnoreAllUnmapped<TSource, TDest>(this IMappingExpression<TSource, TDest> expression)
        {
            expression.ForAllMembers(opt => opt.Ignore());
            return expression;
        }
    }
}
