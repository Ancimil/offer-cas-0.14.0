using MicroserviceCommon.Services;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using PriceCalculation.Calculations;
using PriceCalculation.Exceptions;
using PriceCalculation.Models.Pricing;
using PriceCalculation.Services;
using System.Threading.Tasks;

namespace Offer.Domain.Calculations
{
    public class OfferPriceCalculation : PriceCalculator
    {
        private readonly int maxNumberOfIterations = 5;

        public OfferPriceCalculation(IPriceCalculationService priceCalculationService, IMarketRatesService marketRatesService,
            IConfigurationService configurationService) :
            base(priceCalculationService, marketRatesService, configurationService)
        {
        }

        public async Task<ArrangementRequest> CalculatePrice(ArrangementRequest request, PriceCalculationParameters calcParams)
        {
            decimal amount = 0;
            string term = null;
            if (request is TermLoanRequest termLoanRequest)
            {
                amount = termLoanRequest.Amount;
                term = termLoanRequest.Term;
            }
            var res = await CalculatePrice(calcParams);
            request.MergePriceCalculationResults(res);
            if (request is TermLoanRequest tlRequest)
            {
                #region TermLoanRequest handling
                calcParams.InterestRates = res.InterestRates;
                calcParams.Fees = res.Fees;
                calcParams.OtherConditions = res.OtherConditions;
                var priceCalcualtionChanged = res.ResultChanged;

                tlRequest.ResolveCalculationParameters();

                var iterations = 1; // One iteration already happend (in initial calculation)
                while (priceCalcualtionChanged)
                {
                    if (iterations > maxNumberOfIterations)
                    {
                        throw new MaxNumberOfIterationsException("Maximum number of iterations exceeded.");
                    }
                    res = await CalculatePrice(calcParams);
                    priceCalcualtionChanged = res.ResultChanged;

                    if (term == null)
                    {
                        tlRequest.Term = null;
                    }
                    else if (amount == 0)
                    {
                        tlRequest.Amount = 0;
                    }
                    else
                    {
                        tlRequest.Annuity = 0;
                    }

                    tlRequest.ResolveCalculationParameters();
                    iterations++;
                }
                #endregion
            }
            return request;
        }

        public async Task<ArrangementRequest> CalculatePriceBySimpleCalculation(ArrangementRequest request, PriceCalculationParameters calcParams)
        {
            decimal amount = 0;
            string term = null;
            decimal annuity = 0;
            if (request is TermLoanRequest termLoanRequest)
            {
                amount = termLoanRequest.Amount;
                term = termLoanRequest.Term;
                annuity = termLoanRequest.Annuity;
            }
            var res = await CalculatePrice(calcParams);
            request.MergePriceCalculationResults(res);
            if (request is TermLoanRequest tlRequest)
            {
                #region TermLoanRequest handling
                calcParams.InterestRates = res.InterestRates;
                calcParams.Fees = res.Fees;
                calcParams.OtherConditions = res.OtherConditions;
                var priceCalcualtionChanged = res.ResultChanged;

                var iterations = 1; // One iteration already happend (in initial calculation)
                while (priceCalcualtionChanged)
                {
                    if (iterations > maxNumberOfIterations)
                    {
                        throw new MaxNumberOfIterationsException("Maximum number of iterations exceeded.");
                    }
                    res = await CalculatePrice(calcParams);
                    priceCalcualtionChanged = res.ResultChanged;

                    iterations++;
                }
                #endregion
            }
            return request;
        }

        public async Task<ArrangementRequest> CalculatePrice(Application application, ArrangementRequest request)
        {
            var calcParams = request.GetPriceCalculationParameters(application);
             return await CalculatePrice(request, calcParams);
        }
    }
}
