using System.Linq;
using Autofac;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Infrastructure.Repositories;
using System.Reflection;
using MicroserviceCommon.Infrastructure.Idempotency;
using Offer.API.Services;
using Offer.Domain.Calculations;
using Offer.Domain.Repository;
using Offer.Domain.Utils;
using MicroserviceCommon.Services;
using Offer.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Offer.Domain.Services;
using PriceCalculation.Services;
using Offer.Domain.AggregatesModel.ApplicationAggregate.Interfaces;

namespace Offer.API.Modules
{
    public class ApplicationModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<OfferDBContext>()
                .As<DbContext>()
                .InstancePerDependency();

            builder.RegisterType<ApplicationRepository>()
                .As<IApplicationRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<ApplicationDocumentRepository>()
                .As<IApplicationDocumentRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<InvolvedPartyRepository>()
                .As<IInvolvedPartyRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<ArrangementRequestSnapshotRepository>()
                .As<IArrangementRequestSnapshotRepository>()
                .InstancePerLifetimeScope();

            builder.RegisterType<ArrangementRequestRepository>()
               .As<IArrangementRequestRepository>()
               .InstancePerLifetimeScope();

            builder.RegisterType<QuestionnaireRepository>()
               .As<IQuestionnaireRepository>()
               .InstancePerLifetimeScope();

            builder.RegisterType<RequestManager>()
               .As<IRequestManager>()
               .InstancePerLifetimeScope();

            builder.RegisterType<PriceCalculationService>()
               .As<IPriceCalculationService>()
               .InstancePerLifetimeScope();

            builder.RegisterType<OfferPriceCalculation>()
               .As<OfferPriceCalculation>()
               .InstancePerLifetimeScope();

            builder.RegisterType<MarketRateService>()
               .As<IMarketRatesService>()
               .InstancePerLifetimeScope();

            builder.RegisterType<ContentService>()
               .As<IContentService>()
               .InstancePerLifetimeScope();

            builder.RegisterType<ProductService>()
               .As<IProductService>()
               .InstancePerLifetimeScope();

            builder.RegisterType<CampaignService>()
               .As<ICampaignService>()
               .InstancePerLifetimeScope();

            builder.RegisterType<ArrangementService>()
               .As<IArrangementService>()
               .InstancePerLifetimeScope();

            builder.RegisterType<MasterPartyDataService>()
               .As<IMasterPartyDataService>()
               .InstancePerLifetimeScope();

            builder.RegisterType<FinancialStatementsService>()
                .As<IFinancialStatementsService>()
                .InstancePerLifetimeScope();

            builder.RegisterType<ChangePortfolioRepository>()
                .As<IChangePortfolioRepository>()
                .InstancePerLifetimeScope();

            builder.RegisterType<ProductSnapshotRepository>()
    .As<IProductSnapshotRepository>()
    .InstancePerLifetimeScope();

            builder.RegisterType<ApplicationDocumentsResolver>()
               .As<ApplicationDocumentsResolver>()
               .InstancePerLifetimeScope();

            builder.RegisterType<RequiredDocumentationResolver>()
                .As<RequiredDocumentationResolver>()
                .InstancePerLifetimeScope();

            builder.RegisterType<OfferUtility>()
                .InstancePerLifetimeScope();

            builder.RegisterType<ArrangementRequestFactory>()
                .InstancePerLifetimeScope();

            var dataAccess = Assembly.GetExecutingAssembly();

            builder.RegisterAssemblyTypes(dataAccess)
                .Where(t => t.Name.EndsWith("Handler"))
                .AsImplementedInterfaces();
            //builder.RegisterAssemblyTypes(typeof(InitiateOnlineOfferCommandHandler).GetTypeInfo().Assembly)
            //    .As<InitiateOnlineOfferCommandHandler>();
        }
    }
}
