#region imports
using AuditClient;
using Authorization;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using CalculationService.Calculations;
using CalculationService.Services;
using DigitalToolset.ApiExtensions;
using DigitalToolset.Middlewares;
using MicroserviceCommon.ApiUtil;
using MicroserviceCommon.ApiUtil.Flattening;
using MicroserviceCommon.Application;
using MicroserviceCommon.Extensions.Http.Middleware;
using MicroserviceCommon.Infrastructure.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Offer.API.Mappers;
using Offer.API.Modules;
using Offer.API.Services;
using Offer.Domain.Calculations;
using Offer.Infrastructure;
using Offer.Infrastructure.Services;
using System;
using System.Threading;
using EnvironmentUtils = Offer.API.Application.Helpers.EnvironmentUtils;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
#endregion

namespace Offer
{
    public class Startup : BaseStartup
    {
        public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory) : base(env, loggerFactory)
        {
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            AddMvcWithDynamicCaseResolvers(services);
            LoadSecurity(services);
            LoadEventBus(services);
            LoadCommonServices(services);
            LoadAuditClient(services, GetAuditConfiguration());
            LoadContentService(services);
            services.AddAuthorizationServices();
            EnvironmentUtils.Configuration = Configuration;
            services.AddHostedService<CollateralMessageListener>();
            services.AddHostedService<ContentMessageListener>();
            services.AddHostedService<FinancialStatementsMessageListener>();
            DatabaseConfiguration<OfferDBContext>(services);
            var runningMode = Environment.GetEnvironmentVariable(EnvConst.RunningMode) ?? "normal";
            if (runningMode.ToLower().Equals("organization-unit-sync"))
            {
                using (var context = services.BuildServiceProvider().GetService<OfferDBContext>())
                {
                    for (int i = 0; i < 50; i++)
                    {
                        // Wait for DB to be ready to run OU Sync
                        if (DatabaseInitializer.IsReady(context))
                        {
                            services.AddHostedService<OrganizationUnitSync>();
                            break;
                        }
                        _logger.LogInformation("Database is still not ready. Waiting 5s to Load OU sync");
                        Thread.Sleep(5000);
                    }
                }
            }
            services.AddScoped<Calculator>();
            services.AddSingleton<OrganizationUnitSync>();
            services.AddScoped<CalculationServiceCalculator>();
            services.AddScoped<CalculatorProvider>();
            services.AddDmnRequestExtenderResolver();
            services.AddSwaggerGen();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Offer API",
                    Description = "Offer API lets you initiate, track and execute offers for loans, deposits, credit facilities and current accounts. It provides simulation of conditions for potential products and services. The offer process it defined primarily by the nature of the product or  service being considered, but can include actions such as document checks, collateral allocation, credit assessments, underwriting decisions, regulatory and procedural checks, eligibility checks, the use of internal and external specialist services (such as evaluations and legal advice).",
                    Contact = new OpenApiContact
                    {
                        Name = "Slobodan Amidzic",
                        Email = "slobodan.amidzic@asseco-see.rs",
                        Url = new Uri("https://bankapi.net/docs/public/offerv2-getstarted.html")
                    }
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description =
                    "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });
                // c.DescribeAllEnumsAsStrings();
                c.CustomSchemaIds(x => x.FullName);
            });
            // services.UseAspNetCoreAuditLogServices(Configuration);
            services.AddSwaggerGenNewtonsoftSupport();

            AddCalculationService(services);
            AddHealthChecks<OfferDBContext>(services);
            var container = new ContainerBuilder();
            container.Populate(services);
            container.RegisterModule(new MediatorModule());
            container.RegisterModule(new ApplicationModule());
            ConfigureMapper.Configure(services.BuildServiceProvider());
            var result = new AutofacServiceProvider(container.Build());
            return result;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IServiceProvider provider)
        {
            UseEventBus(provider);
            UseCommonMiddlewares(app);
            CorsConfiguration(app);
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseFlatteningMiddleware();
            app.UseAdditionalFieldsMiddleware();
            app.UseShapingMiddleware();
            app.UseSwagger(c => { c.RouteTemplate = "v1/offer/swagger/{documentName}/swagger.json"; });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/v1/offer/swagger/v1/swagger.json", "Offer API V1");
                c.RoutePrefix = "v1/offer/swagger";
            });
            UseForwardHeaders(app);
            UseTranslationMiddleware(app);
            app.UseMvc();
            UseHealthChecks(app);
            InitializeDatabaseDeveloper(app);

            ConfigureAuditClient(provider);
        }

        private void AddCalculationService(IServiceCollection services)
        {
            services.Configure<CalculationServiceOptions>(opt =>
            {
                opt.ArrangementType = EnvironmentUtils.GetVariable("CS_ARRANGEMENT_TYPE", "CalculationService:ArrangementType");
                opt.ClientIdentifier = EnvironmentUtils.GetVariable("CS_CLIENT_IDENTIFIER", "CalculationService:ClientIdentifier");
                opt.RouteIdentifier = EnvironmentUtils.GetVariable("CS_ROUTE_IDENTIFIER", "CalculationService:RouteIdentifier");
                opt.Url = EnvironmentUtils.GetVariable("CS_URL", "CalculationService:URL");
            });
        }
        public AuditClientConfigurationBuilder GetAuditConfiguration()
        {
            var auditConfigBuilder = new AuditClientConfigurationBuilder();

            return auditConfigBuilder;
        }

    }


}
