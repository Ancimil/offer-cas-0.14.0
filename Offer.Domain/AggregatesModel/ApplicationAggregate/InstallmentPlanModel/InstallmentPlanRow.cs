using System;
using System.Collections.Generic;
using InstallmentPlanRowCS = CalculationService.Models.InstallmentPlanRow;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{

    public class InstallmentPlanRow
    {
        public int Ordinal { get; set; }
        public DateTime Date { get; set; }
        public ActivityKind ActivityKind { get; set; }
        public string Description { get; set; }
        public decimal Disbursement { get; set; }
        public decimal StartingBalance { get; set; }
        public decimal PrincipalRepayment { get; set; }
        public decimal InterestRepayment { get; set; }
        public decimal Annuity { get; set; }
        public decimal OutstandingBalance { get; set; }
        public decimal Fee { get; set; }
        public decimal OtherExpenses { get; set; }
        public decimal CashCollateral { get; set; }
        public decimal DeferredPayment { get; set; }
        public decimal DeferredPaymentInstallment { get; set; }
        public decimal NetCashFlow { get; set; }
        public decimal DiscountedNetCashFlow { get; set; }
        public decimal DiscountedDisbursement { get; set; }
        public decimal DiscountedCashCollateralFlow { get; set; }
        public decimal YearFrac { get; set; }

        public static InstallmentPlanRow FromInstallmentPlanCS(InstallmentPlanRowCS installmentPlanRowCS)
        {
            if (!Enum.TryParse(installmentPlanRowCS.ActivityKind, true, out ActivityKind activityKind))
            {
                activityKind = ActivityKind.Repayment;
            }
            var planRow = new InstallmentPlanRow
            {
                ActivityKind = activityKind,
                Annuity = (decimal)(installmentPlanRowCS.Annuity ?? 0),
                CashCollateral = (decimal)(installmentPlanRowCS.CashCollateral ?? 0),
                Date = installmentPlanRowCS.Date ?? DateTime.Today,
                DeferredPayment = (decimal)(installmentPlanRowCS.DeferredPayment ?? 0),
                DeferredPaymentInstallment = (decimal)(installmentPlanRowCS.DeferredPaymentInstallment ?? 0),
                Description = installmentPlanRowCS.Description,
                Disbursement = (decimal)(installmentPlanRowCS.Disbursement ?? 0),
                DiscountedCashCollateralFlow = (decimal)(installmentPlanRowCS.DiscountedCashCollateralFlow ?? 0),
                DiscountedDisbursement = (decimal)(installmentPlanRowCS.DiscountedDisbursement ?? 0),
                DiscountedNetCashFlow = (decimal)(installmentPlanRowCS.DiscountedNetCashFlow ?? 0),
                Fee = (decimal)(installmentPlanRowCS.Fee ?? 0),
                InterestRepayment = (decimal)(installmentPlanRowCS.InterestRepayment ?? 0),
                NetCashFlow = (decimal)(installmentPlanRowCS.NetCashFlow ?? 0),
                Ordinal = installmentPlanRowCS.Ordinal ?? 0,
                OtherExpenses = (decimal)(installmentPlanRowCS.OtherExpenses ?? 0),
                OutstandingBalance = (decimal)(installmentPlanRowCS.OutstandingBalance ?? 0),
                PrincipalRepayment = (decimal)(installmentPlanRowCS.PrincipalRepayment ?? 0),
                StartingBalance = (decimal)(installmentPlanRowCS.StartingBalance ?? 0),
                YearFrac = 0
            };

            return planRow;
        }


        public static List<InstallmentPlanRow> FromInstallmentPlanCSList(List<InstallmentPlanRowCS> installmentPlanRowCSList)
        {
            var list = new List<InstallmentPlanRow>();
            installmentPlanRowCSList.ForEach(p => list.Add(FromInstallmentPlanCS(p)));
            return list;
        }
    }
}
