using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using OfferApplication = Offer.Domain.AggregatesModel.ApplicationAggregate.Application;
using MicroserviceCommon.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Offer.Domain.AggregatesModel.ApplicationAggregate.ProductModel;
using System.Security.Cryptography;
using System.Text;

namespace Offer.Domain.Utils
{
    public class OfferUtility
    {
        private readonly IConfigurationService _configurationService;
        private readonly IMediator _mediator;
        private readonly ILogger<OfferUtility> _logger;
        private readonly ArrangementRequestFactory _requestFactory;

        public OfferUtility(IConfigurationService configurationService, IMediator mediator, ILogger<OfferUtility> logger,
            ArrangementRequestFactory requestFactory)
        {
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _requestFactory = requestFactory ?? throw new ArgumentNullException(nameof(requestFactory));
        }

        public static ArrangementKind? GetArrangmentKindByProductKind(ProductKinds kind)
        {
            switch (kind)
            {
                case ProductKinds.CardAccessProduct:
                    return ArrangementKind.CardAccessArrangement;
                case ProductKinds.CreditFacilityProduct:
                    return ArrangementKind.CreditFacility;
                case ProductKinds.CurrentAccountProduct:
                    return ArrangementKind.CurrentAccount;
                case ProductKinds.DemandDepositProduct:
                    return ArrangementKind.DemandDeposit;
                case ProductKinds.ElectronicAccessProduct:
                    return ArrangementKind.ElectronicAccessArrangement;
                case ProductKinds.OverdraftFacilityProduct:
                    return ArrangementKind.OverdraftFacility;
                case ProductKinds.TermDepositProduct:
                    return ArrangementKind.TermDeposit;
                case ProductKinds.TermLoanProduct:
                    return ArrangementKind.TermLoan;
                case ProductKinds.CreditCardFacilityProduct:
                    return ArrangementKind.CreditCardFacility;
                case ProductKinds.AbstractProduct:
                    return ArrangementKind.Abstract;
                case ProductKinds.Service:
                    return ArrangementKind.OtherProductArrangement;
                case ProductKinds.CreditLineProduct:
                    return ArrangementKind.CreditLine;
                default:
                    return ArrangementKind.OtherProductArrangement;
            }
        }

        public async Task<List<ArrangementRequest>> ResolveAdditionalProduct(OfferApplication application, ApplyForProductDefinition message)
        {
            #region Fetch additional product settings from configuration
            var additionalProductSettings = await _configurationService.GetEffective<AdditionalProductSettings>("offer/new-customer/default-addon-settings");
            if (string.IsNullOrEmpty(additionalProductSettings?.ProductCode))
            {
                _logger.LogWarning("ProductCode for Additional Product Settings doesn't exist. Using additional product for new customer is skipped.");
                return null;
            }
            #endregion

            try
            {
                // Potencijalni problem - kreiranje ArrangementRequestInitializationParameters
                var parameters = new ArrangementRequestInitializationParameters
                {
                    CustomerNumber = message.CustomerNumber,
                    Currency = message.Currency,
                    ProductCode = additionalProductSettings.ProductCode
                };
                return await _requestFactory.AddToApplication(application, additionalProductSettings.ProductCode, parameters);
            }
            catch (HttpRequestException)
            {
                _logger.LogInformation("Additional product for product code {ProductCode} doesn't exist. Using additional product for new customer is skipped.",
                    additionalProductSettings.ProductCode);
                return null;
            }
        }

        public static string CreateMD5(string inputString)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytesFromString = Encoding.ASCII.GetBytes(inputString);
                byte[] hashBytes = md5.ComputeHash(inputBytesFromString);

                StringBuilder sb = new StringBuilder();
                foreach (var hashByte in hashBytes)
                {
                    sb.Append(hashByte.ToString("X2"));
                }
                return sb.ToString();
            }
        }

    }
}
