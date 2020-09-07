using MediatR;
using MicroserviceCommon.Application.Commands;
using MicroserviceCommon.Infrastructure.Idempotency;
using MicroserviceCommon.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.AggregatesModel.ApplicationAggregate.ApplicantModel;
using Offer.Domain.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Offer.API.Application.Commands
{

    public class UpdateFinancialStatementsCommandHandler : IRequestHandler<UpdateFinancialStatementsCommand, bool>
    {
        private readonly ILogger<UpdateFinancialStatementsCommand> _logger;
        private readonly IInvolvedPartyRepository _involvedPartyRepository;
        private readonly IFinancialStatementsService _financialStatementsService;
        private IConfigurationService _configurationService { get; set; }

        public UpdateFinancialStatementsCommandHandler(ILogger<UpdateFinancialStatementsCommand> logger,
                                                       IInvolvedPartyRepository involvedPartyRepository,
                                                       IFinancialStatementsService financialStatementsService,
                                                       IConfigurationService configurationService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _involvedPartyRepository = involvedPartyRepository ?? throw new ArgumentNullException(nameof(involvedPartyRepository));
            _financialStatementsService = financialStatementsService;
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        }

        public async Task<bool> Handle(UpdateFinancialStatementsCommand message, CancellationToken cancellationToken)
        {
            var mandatoryReports = await _configurationService.GetEffective("financial-statements/mandatory-reports");
            JObject json = JObject.Parse(mandatoryReports);
            JArray list = (JArray)json["mandatory-reports"];

            foreach (var involvedPartyItem in message.Application.InvolvedParties)
            {
                if (involvedPartyItem is OrganizationParty organization)
                {

                    var fslist = new List<FinancialStatement>();
                    organization.FinancialStatements = new List<FinancialStatement>();
                    foreach (var item in list)
                    {
                        JArray reports = (JArray)item["reports"];
                        var distinctYears = (from foo in reports
                                             select foo["year"]).Distinct().ToArray();
                        foreach (var year in distinctYears)
                        {
                            FinancialStatement statement = new FinancialStatement();
                            statement.Reports = new List<Report>();
                            statement.Year = Convert.ToInt32(year);
                            foreach (var statementItem in reports)
                            {
                                if (Convert.ToInt32(statementItem["year"]) == Convert.ToInt32(year))
                                {

                                    FinancialStatementsData financialStatementsData = await _financialStatementsService.GetFinancialStatementAsync(organization.CustomerNumber, statementItem["report-type"].ToString(), statementItem["accounting-method"].ToString(), Convert.ToInt32(statementItem["year"]));
                                    Report r = new Report
                                    {
                                        ReportId = (financialStatementsData.ReportId != 0) ? financialStatementsData.ReportId : null,
                                        ReportType = statementItem["report-type"].ToString(),
                                        AccountingMethod = statementItem["accounting-method"].ToString(),
                                        ReportDate = financialStatementsData.ReportDate
                                    };
                                    statement.Reports.Add(r);
                                }
                            }
                            fslist.Add(statement);
                        }
                    }
                    organization.FinancialStatements = fslist;
                }
            }

            _involvedPartyRepository.PublishFinancialStatementsChangeEvent(message.Application.ApplicationNumber,
               message.Application.CustomerNumber);
            return true;
        }
    }

    public class UpdateFinancialStatementsIdentifiedCommandHandler : IdentifiedCommandHandler<UpdateFinancialStatementsCommand, bool>
    {
        public UpdateFinancialStatementsIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {
        }

        protected override bool CreateResultForDuplicateRequest()
        {
            return true;
        }
    }

}
