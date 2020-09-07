using CalculationService.Models;
using CalculationService.Services;
using CalcService = CalculationService.Services.CalculationService;
//using CalcPlanRequest = CalculationService.Models.CalculateInstallmentPlanRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using AutoMapper;
using System.ServiceModel;
using PriceCalculation.Calculations;

namespace CalculationService.Calculations
{
    public class Calculator
    {
        private readonly IOptions<CalculationServiceOptions> _options;

        public Calculator(IOptions<CalculationServiceOptions> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public InstallmentPlan CalculateInstallmentPlan(CalculateInstallmentPlanRequest request)
        {
            #region Request Validation
            int termUnits = 0;
            SimpleUnitOfTime termUnitOfTime = SimpleUnitOfTime.M;

            if (!string.IsNullOrEmpty(request.Term))
            {
                termUnits = Utility.GetMonthsFromPeriod(request.Term);
                if (termUnits <= 0)
                {
                    termUnits = Utility.GetDaysFromPeriod(request.Term);
                    termUnitOfTime = SimpleUnitOfTime.D;
                }
                // string tmp = request.Term.Trim();
                /*string uot = request.Term.Substring(request.Term.Length - 1);
                string trm = request.Term.Substring(0, request.Term.Length - 1);
                if (!int.TryParse(trm, out termUnits) || !SimpleUnitOfTime.TryParse(uot, out termUnitOfTime) || termUnits == 0)
                {
                    // vp.Add(new ValidationproblemInner() { Field = "term", Errors = new List<ValidationproblemInnerErrors>() { new ValidationproblemInnerErrors() { Error = "002", Message = "Invalid term!" } } });
                }*/
            }
            else if (request.FixedAnnuity == 0)
            {
                // vp.Add(new ValidationproblemInner() { Field = "fixedAnnuity", Errors = new List<ValidationproblemInnerErrors>() { new ValidationproblemInnerErrors() { Error = "003", Message = "If term is not specified, then fixedAnnuity must be!" } } });
            }
            #endregion

            var calcReq = PrepareInstallmentPlanRequest(request, termUnits, termUnitOfTime);
            var res = CalculateInstalmentPlanCS(calcReq);

            var plan = GetPlanFromCalculationResponse(res.CalculationResponse);

            return plan;
        }

        private InstallmentPlanRow GetPlanRowFromDataRow(System.Data.DataRow dataRow)
        {
            return new InstallmentPlanRow
            {
                Ordinal = dataRow["Ordinal"] as int? ?? 0,
                Date = dataRow["Date"] as DateTime?,
                Description = dataRow["Description"] as string,
                Disbursement = (double)(dataRow["Disbursement"] as decimal?).GetValueOrDefault(),
                StartingBalance = (double)(dataRow["Principal"] as decimal?).GetValueOrDefault(),
                PrincipalRepayment = (double)(dataRow["Principal repayment"] as decimal?).GetValueOrDefault(),
                InterestRepayment = (double)(dataRow["Interest repayment"] as decimal?).GetValueOrDefault(),
                Annuity = (double)(dataRow["Annuity"] as decimal?).GetValueOrDefault(),
                OutstandingBalance = (double)(dataRow["Outstanding"] as decimal?).GetValueOrDefault(),
                Fee = (double)(dataRow["Fee"] as decimal?).GetValueOrDefault(),
                OtherExpenses = (double)(dataRow["Other Expenses"] as decimal?).GetValueOrDefault(),
                CashCollateral = (double)(dataRow["Cash Collateral"] as decimal?).GetValueOrDefault(),
                DeferredPayment = (double)(dataRow["Deferred Installment"] as decimal?).GetValueOrDefault(),
                DeferredPaymentInstallment = (double)(dataRow["Repayment of Deferred Installment"] as decimal?).GetValueOrDefault(),
                ActivityKind = dataRow["ActivityGroupId"] as string,
                NetCashFlow = (double)(dataRow["NetCashFlow"] as decimal?).GetValueOrDefault(),
                DiscountedNetCashFlow = (double)(dataRow["DiscountedNetCashFlow"] as decimal?).GetValueOrDefault()
            };
        }

        private InstallmentPlan GetPlanFromCalculationResponse(ObligationCalculationResult response)
        {
            InstallmentPlan plan = new InstallmentPlan
            {
                Annuity = (double)response.InstallmentPlanCalculationResult.AnnuityInfo.Where(ai => ai.AnnuitySpecified && ai.Annuity != 0).Select(it => it.Annuity).FirstOrDefault()
            };
            if (response.InstallmentPlanCalculationResult.EffectiveInterestRateSpecified)
                plan.EffectiveInterestRate = (double)response.InstallmentPlanCalculationResult.EffectiveInterestRate.GetValueOrDefault();

            int cnt = 0;
            DateTime lastDate = DateTime.MinValue;
            plan.Installments = new List<InstallmentPlanRow>();

            foreach (System.Data.DataRow item in response.InstallmentPlanCalculationResult.InstallmentPlan.Rows)
            {
                var r = GetPlanRowFromDataRow(item);

                if (r.Disbursement != 0 || r.Annuity != 0 || r.Fee != 0 || r.OtherExpenses != 0)
                {
                    plan.Installments.Add(r);
                }

                if (r.PrincipalRepayment != 0)
                {
                    cnt++;
                    lastDate = r.Date.GetValueOrDefault();
                }
            }

            plan.NumberOfInstallments = cnt;
            return plan;
        }

        public CalculateArrangementActivityPlanKDPRs CalculateInstalmentPlanCS(KdpInstallmentPlanCalculationRequest request)
        {
            var endpointUrl = _options.Value.Url; // http://expapp1:54505/CalculationService
            BasicHttpBinding binding = new BasicHttpBinding
            {
                MaxReceivedMessageSize = 2147483647
            };
            EndpointAddress endpoint = new EndpointAddress(endpointUrl);
            ChannelFactory<CalcService> channelFactory = new ChannelFactory<CalcService>(binding, endpoint);
            CalcService clientProxy = channelFactory.CreateChannel();
            var res = clientProxy.CalculateArrangementActivityPlanKDP(new CalculateArrangementActivityPlanKDPRq(request));
            return res;
        }

        private KdpInstallmentPlanCalculationRequest PrepareInstallmentPlanRequest(CalculateInstallmentPlanRequest request, int termUnits = 0,
            SimpleUnitOfTime termUnitOfTime = SimpleUnitOfTime.M)
        {
            KdpInstallmentPlanCalculationRequest calcReq = Mapper.Map<CalculateInstallmentPlanRequest, KdpInstallmentPlanCalculationRequest>(request);

            if (termUnits != 0)
            {
                switch (termUnitOfTime)
                {
                    case SimpleUnitOfTime.Y:
                        calcReq.NumberOfInstallments = termUnits * 12;
                        break;
                    case SimpleUnitOfTime.D:
                        calcReq.NumberOfInstallments = termUnits / 30; // aproksimiramo
                        break;
                    default:
                        calcReq.NumberOfInstallments = termUnits;
                        break;
                }
            }
            else
            {
                calcReq.NumberOfInstallments = 9999;
            }
            calcReq.NumberOfInstallmentsSpecified = true;

            if (termUnits != 0)
            {
                switch (termUnitOfTime)
                {
                    case SimpleUnitOfTime.Y:
                        calcReq.MaturityDate = calcReq.StartDate.AddYears(termUnits);
                        break;
                    case SimpleUnitOfTime.D:
                        calcReq.MaturityDate = calcReq.StartDate.AddDays(termUnits);
                        break;
                    default:
                        calcReq.MaturityDate = calcReq.StartDate.AddMonths(termUnits);
                        break;
                }
            }
            else
            {
                calcReq.MaturityDate = calcReq.StartDate.AddYears(100);
            }
            calcReq.MaturityDateSpecified = true;

            // da postavim sve datume koje sam zaboravio :)
            foreach (var prop in typeof(KdpInstallmentPlanCalculationRequest).GetProperties().Where(p => p.PropertyType == typeof(DateTime)))
            {
                prop.SetValue(calcReq, ((DateTime)prop.GetValue(calcReq)).Date);
                if (((DateTime)prop.GetValue(calcReq)).Date < calcReq.StartDate.Date)
                    prop.SetValue(calcReq, calcReq.StartDate.Date);
                typeof(KdpInstallmentPlanCalculationRequest).GetProperty(prop.Name + "Specified").SetValue(calcReq, true);
            }
            calcReq.AdditionalRegularInterestInfo = GetAdditionalRegularInterestInfo(request.Periods, calcReq.StartDate, calcReq.MaturityDate,
                calcReq.RegularInterestPercentage, calcReq.RegularInterestUnitOfTime);
            calcReq.AdditionalConditions = calcReq.AdditionalRegularInterestInfo != null && calcReq.AdditionalRegularInterestInfo.Count() > 0;
            calcReq.AdditionalConditionsSpecified = calcReq.AdditionalRegularInterestInfo != null && calcReq.AdditionalRegularInterestInfo.Count() > 0;

            calcReq.RouteIdentifier = _options.Value.RouteIdentifier;
            calcReq.ArrangementType = _options.Value.ArrangementType;
            calcReq.ClientIdentifier = _options.Value.ClientIdentifier;
            calcReq.InstallmentPlanCalculationScenario = 2;
            calcReq.InstallmentPlanCalculationScenarioSpecified = true;

            return calcReq;
        }

        private SimpleInterestRateInfo[] GetAdditionalRegularInterestInfo(List<PricedScheduledPeriod> periods,
            DateTime startDate, DateTime maturityDate, double percentage, SimpleUnitOfTime unitOfTime)
        {
            if (periods == null || periods.Count() == 0)
            {
                return null;
            }
            periods = periods.OrderBy(p => p.StartDate).ToList();
            DateTime runningDate = startDate.Date;
            var info = new List<SimpleInterestRateInfo>();
            foreach (var period in periods)
            {
                if (startDate.Date > period.StartDate.Date || startDate.Date > period.EndDate.Date)
                {
                    throw new ArgumentOutOfRangeException("All periods must start after start date.");
                }
                else if (runningDate.Date < period.StartDate.Date)
                {
                    info.Add(new SimpleInterestRateInfo
                    {
                        Date = runningDate.Date,
                        UnitOfTime = unitOfTime,
                        Percentage = percentage,
                        DateSpecified = true,
                        UnitOfTimeSpecified = true,
                        PercentageSpecified = true
                    });
                    runningDate = period.EndDate.Date;
                    info.Add(new SimpleInterestRateInfo
                    {
                        Date = period.StartDate.Date,
                        UnitOfTime = period.UnitOfTime,
                        Percentage = period.Percentage,
                        DateSpecified = true,
                        UnitOfTimeSpecified = true,
                        PercentageSpecified = true
                    });
                }
                else if (runningDate.Date.Equals(period.StartDate.Date))
                {
                    runningDate = period.EndDate.Date;
                    info.Add(new SimpleInterestRateInfo
                    {
                        Date = period.StartDate.Date,
                        UnitOfTime = period.UnitOfTime,
                        Percentage = period.Percentage,
                        DateSpecified = true,
                        UnitOfTimeSpecified = true,
                        PercentageSpecified = true
                    });
                }
            }
            if (runningDate.Date < maturityDate.Date)
            {
                info.Add(new SimpleInterestRateInfo
                {
                    Date = runningDate.Date,
                    UnitOfTime = unitOfTime,
                    Percentage = percentage,
                    DateSpecified = true,
                    UnitOfTimeSpecified = true,
                    PercentageSpecified = true
                });
            }
            return info.ToArray();
        }
    }
}
