using Offer.Domain.Calculations;
using PriceCalculation.Models.Lifecycle;
using PriceCalculation.Models.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static Offer.Domain.Calculations.InstallmentPlanCalculation;

namespace FinancialCalculationTest
{
    public class CalculatorTest
    {
        [Fact]
        public void TestSimpleInstallmentPlanCalculationCase1()
        {
            SimpleLoanCalculationRequest request = new SimpleLoanCalculationRequest();
            request.Amount = (decimal)16741.12;
            request.Currency = "EUR";
            request.StardDate = new DateTime(2019, 10, 24);
            request.RegularInterest = new List<InterestRateEntry>();
            request.RegularInterest.Add(new InterestRateEntry() { 
                Date = request.StardDate,
                RatePercentage = 3.990,
                IsCompound = false,
                CalendarBasis = CalendarBasisKind.Calendar30E360, 
                RateUnitOfTime = SimpleUnitOfTime.Y,
                Name = "Kamata"
            });
            request.InstallmentSchedule = new SimpleSchedule();
            request.InstallmentSchedule.DayOfMonth = 5;
            request.MinimumDaysForFirstInstallment = 20;
            request.NumberOfInstallments = 84;
            request.RepaymentType = RepaymentType.FixedAnnuity;
            request.InstallmentSchedule.FrequencyPeriod = 1;
            request.InstallmentSchedule.FrequencyUnitOfTime = SimpleUnitOfTime.M;
            request.IntercalarInterestRepaymentType = IntercalarInterestRepaymentType.WithFirstInstallment;

            request.Fees = new List<FeeEntry>();
            request.Fees.Add(new FeeEntry() {
                Percentage = (decimal)1.5,
                Date = request.StardDate,
                Kind = FeeConditionKind.OriginationFee,
                Frequency = FeeConditionFrequency.EventTriggered,
                CalculationBasisType = CalculationBasisType.ContractAmount,
                Name = "Naknada za obradu kreditnog zahteva"
            });

            request.Fees.Add(new FeeEntry()
            {
                FixedAmount = (decimal)2.47,
                Currency = "EUR",
                Date = new DateTime(2019, 12, 5),
                Kind = FeeConditionKind.Expense,
                Frequency = FeeConditionFrequency.Monthly,
                CalculationBasisType = CalculationBasisType.PrecalculatedAmount,
                Name = "Trosak otvaranja i vodjenja racuna"
            });

            request.Fees.Add(new FeeEntry()
            {
                FixedAmount = (decimal)(673.01 - 2.47),
                Currency = "EUR",
                Date = new DateTime(2019, 12, 5),
                Kind = FeeConditionKind.Expense,
                Frequency = FeeConditionFrequency.Yearly,
                CalculationBasisType = CalculationBasisType.PrecalculatedAmount,
                Name = "Osiguranje nepokretnosti"
            });

            request.AdjustFirstInstallment = true; // strange logic, but ok...

            var result = InstallmentPlanCalculation.CalculateInstallmentPlan(request);
            var someInr = result.Rows.Where(it => it.Date == new DateTime(2020, 3, 5)).First().InterestRepayment;
            var sumRepayment = result.Rows.Sum(it => it.PrincipalRepayment);
            Assert.True(sumRepayment == request.Amount);
            Assert.True(result.Rows.Count == 85);
            Assert.True(result.Rows.Last().PrincipalRepayment == (decimal)228.37);
            Assert.True(Math.Round(result.APR, 2) == (decimal)12.47);
            Assert.True(Math.Round(result.Annuity, 2) == (decimal)228.75);
            Assert.True(Math.Round(someInr, 2) == (decimal)53.93);
        }
        [Fact]
        public void TestSimpleInstallmentPlanCalculationCase1b()
        {
            SimpleLoanCalculationRequest request = new SimpleLoanCalculationRequest();
            //request.Amount = (decimal)16741.12;
            request.Annuity = (decimal)228.75;
            request.CalculationTarget = CalculationTarget.Amount;
            request.Currency = "EUR";
            request.StardDate = new DateTime(2019, 10, 24);
            request.RegularInterest = new List<InterestRateEntry>();
            request.RegularInterest.Add(new InterestRateEntry()
            {
                Date = request.StardDate,
                RatePercentage = 3.990,
                IsCompound = false,
                CalendarBasis = CalendarBasisKind.Calendar30E360,
                RateUnitOfTime = SimpleUnitOfTime.Y,
                Name = "Kamata"
            });
            request.InstallmentSchedule = new SimpleSchedule();
            request.InstallmentSchedule.DayOfMonth = 5;
            request.MinimumDaysForFirstInstallment = 20;
            request.NumberOfInstallments = 84;
            request.RepaymentType = RepaymentType.FixedAnnuity;
            request.InstallmentSchedule.FrequencyPeriod = 1;
            request.InstallmentSchedule.FrequencyUnitOfTime = SimpleUnitOfTime.M;
            request.IntercalarInterestRepaymentType = IntercalarInterestRepaymentType.WithFirstInstallment;

            request.Fees = new List<FeeEntry>();
            request.Fees.Add(new FeeEntry()
            {
                Percentage = (decimal)1.5,
                Date = request.StardDate,
                Kind = FeeConditionKind.OriginationFee,
                Frequency = FeeConditionFrequency.EventTriggered,
                CalculationBasisType = CalculationBasisType.ContractAmount,
                Name = "Naknada za obradu kreditnog zahteva"
            });

            request.Fees.Add(new FeeEntry()
            {
                FixedAmount = (decimal)2.47,
                Currency = "EUR",
                Date = new DateTime(2019, 12, 5),
                Kind = FeeConditionKind.Expense,
                Frequency = FeeConditionFrequency.Monthly,
                CalculationBasisType = CalculationBasisType.PrecalculatedAmount,
                Name = "Trosak otvaranja i vodjenja racuna"
            });

            request.Fees.Add(new FeeEntry()
            {
                FixedAmount = (decimal)(673.01 - 2.47),
                Currency = "EUR",
                Date = new DateTime(2019, 12, 5),
                Kind = FeeConditionKind.Expense,
                Frequency = FeeConditionFrequency.Yearly,
                CalculationBasisType = CalculationBasisType.PrecalculatedAmount,
                Name = "Osiguranje nepokretnosti"
            });

            request.AdjustFirstInstallment = true; // strange logic, but ok...

            var result = InstallmentPlanCalculation.CalculateInstallmentPlan(request);
            var someInr = result.Rows.Where(it => it.Date == new DateTime(2020, 3, 5)).First().InterestRepayment;
            var sumRepayment = result.Rows.Sum(it => it.PrincipalRepayment);
            Assert.True(sumRepayment == request.Amount);
            Assert.True(result.Rows.Count == 85);
            Assert.True(result.Rows.Last().PrincipalRepayment == (decimal)227.96);
            Assert.True(Math.Round(result.APR, 2) == (decimal)12.47);
            Assert.True(Math.Round(result.Amount, 2) == (decimal)16740.82);
            Assert.True(Math.Round(someInr, 2) == (decimal)53.93);
        }
        [Fact]
        public void TestSimpleInstallmentPlanCalculationCase29th()
        {
            SimpleLoanCalculationRequest request = new SimpleLoanCalculationRequest();
            request.Amount = (decimal)200000.00;
            //request.Annuity = (decimal)228.75;
            request.CalculationTarget = CalculationTarget.Annuity;
            request.Currency = "RSD";
            request.StardDate = new DateTime(2020, 1, 29);
            request.RegularInterest = new List<InterestRateEntry>();
            request.RegularInterest.Add(new InterestRateEntry()
            {
                Date = request.StardDate,
                RatePercentage = 7.95,
                IsCompound = false,
                CalendarBasis = CalendarBasisKind.Calendar30E360,
                RateUnitOfTime = SimpleUnitOfTime.Y,
                Name = "Kamata"
            });
            request.InstallmentSchedule = new SimpleSchedule();
            request.InstallmentSchedule.DayOfMonth = 5;
            request.MinimumDaysForFirstInstallment = 20;
            request.NumberOfInstallments = 36;
            request.RepaymentType = RepaymentType.FixedAnnuity;
            request.InstallmentSchedule.FrequencyPeriod = 1;
            request.InstallmentSchedule.FrequencyUnitOfTime = SimpleUnitOfTime.M;
            request.IntercalarInterestRepaymentType = IntercalarInterestRepaymentType.WithFirstInstallment;

            request.Fees = new List<FeeEntry>();
            request.Fees.Add(new FeeEntry()
            {
                Percentage = (decimal)2,
                Date = request.StardDate,
                Kind = FeeConditionKind.OriginationFee,
                Frequency = FeeConditionFrequency.EventTriggered,
                CalculationBasisType = CalculationBasisType.ContractAmount,
                Name = "Naknada za obradu kreditnog zahteva"
            });


            request.Fees.Add(new FeeEntry()
            {
                FixedAmount = (decimal)245.00,
                Currency = "RSD",
                Date = new DateTime(2020, 1, 29),
                Kind = FeeConditionKind.Expense,
                Frequency = FeeConditionFrequency.EventTriggered,
                CalculationBasisType = CalculationBasisType.PrecalculatedAmount,
                Name = "Trošak KB"
            });

            request.AdjustFirstInstallment = true; // strange logic, but ok...
            request.InterestSchedule = null;
            var result = InstallmentPlanCalculation.CalculateInstallmentPlan(request);
            var someInr = result.Rows.Where(it => it.Date == new DateTime(2020, 3, 5)).First().InterestRepayment;
            var sumRepayment = result.Rows.Sum(it => it.PrincipalRepayment);
            Assert.True(sumRepayment == request.Amount);
            Assert.True(result.Rows.Count == 37);
            Assert.True(result.Rows.Last().PrincipalRepayment == (decimal)6221.49);
            Assert.True(Math.Round(result.APR, 2) == (decimal)9.82);
            Assert.True(Math.Round(result.Annuity, 2) == (decimal)6262.66);
            Assert.True(Math.Round(someInr, 2) == (decimal)1590.00);
        }
        [Fact]
        public void TestSimpleInstallmentPlanCalculationCase1c()
        {
            SimpleLoanCalculationRequest request = new SimpleLoanCalculationRequest();
            request.Amount = (decimal)16741.12;
            request.Annuity = (decimal)228.75;
            //request.Annuity = (decimal)229;
            request.CalculationTarget = CalculationTarget.Term;
            request.Currency = "EUR";
            request.StardDate = new DateTime(2019, 10, 24);
            request.RegularInterest = new List<InterestRateEntry>();
            request.RegularInterest.Add(new InterestRateEntry()
            {
                Date = request.StardDate,
                RatePercentage = 3.990,
                IsCompound = false,
                CalendarBasis = CalendarBasisKind.Calendar30E360,
                RateUnitOfTime = SimpleUnitOfTime.Y,
                Name = "Kamata"
            });
            request.InstallmentSchedule = new SimpleSchedule();
            request.InstallmentSchedule.DayOfMonth = 5;
            request.MinimumDaysForFirstInstallment = 20;
            //request.NumberOfInstallments = 84;
            request.RepaymentType = RepaymentType.FixedAnnuity;
            request.InstallmentSchedule.FrequencyPeriod = 1;
            request.InstallmentSchedule.FrequencyUnitOfTime = SimpleUnitOfTime.M;
            request.IntercalarInterestRepaymentType = IntercalarInterestRepaymentType.WithFirstInstallment;

            request.Fees = new List<FeeEntry>();
            request.Fees.Add(new FeeEntry()
            {
                Percentage = (decimal)1.5,
                Date = request.StardDate,
                Kind = FeeConditionKind.OriginationFee,
                Frequency = FeeConditionFrequency.EventTriggered,
                CalculationBasisType = CalculationBasisType.ContractAmount,
                Name = "Naknada za obradu kreditnog zahteva"
            });

            request.Fees.Add(new FeeEntry()
            {
                FixedAmount = (decimal)2.47,
                Currency = "EUR",
                Date = new DateTime(2019, 12, 5),
                Kind = FeeConditionKind.Expense,
                Frequency = FeeConditionFrequency.Monthly,
                CalculationBasisType = CalculationBasisType.PrecalculatedAmount,
                Name = "Trosak otvaranja i vodjenja racuna"
            });

            request.Fees.Add(new FeeEntry()
            {
                FixedAmount = (decimal)(673.01 - 2.47),
                Currency = "EUR",
                Date = new DateTime(2019, 12, 5),
                Kind = FeeConditionKind.Expense,
                Frequency = FeeConditionFrequency.Yearly,
                CalculationBasisType = CalculationBasisType.PrecalculatedAmount,
                Name = "Osiguranje nepokretnosti"
            });

            request.AdjustFirstInstallment = true; // strange logic, but ok...

            var result = InstallmentPlanCalculation.CalculateInstallmentPlan(request);
            var someInr = result.Rows.Where(it => it.Date == new DateTime(2020, 3, 5)).First().InterestRepayment;
            var sumRepayment = result.Rows.Sum(it => it.PrincipalRepayment);
            Assert.True(sumRepayment == request.Amount);
            Assert.True(result.Rows.Count == 85);
            Assert.True(result.Rows.Last().PrincipalRepayment == (decimal)228.37);
            Assert.True(Math.Round(result.APR, 2) == (decimal)12.47);
            Assert.True(result.NumberOfInstallments == 84);
            Assert.True(Math.Round(someInr, 2) == (decimal)53.93);
        }
        [Fact]
        public void TestSimpleInstallmentPlanCalculationCase2()
        {
            SimpleLoanCalculationRequest request = new SimpleLoanCalculationRequest();
            request.Amount = (decimal)4060.92;
            request.Currency = "EUR";
            request.StardDate = new DateTime(2019, 10, 30);
            request.RegularInterest = new List<InterestRateEntry>();
            request.RegularInterest.Add(new InterestRateEntry()
            {
                Date = request.StardDate,
                RatePercentage = 3.990,
                IsCompound = false,
                CalendarBasis = CalendarBasisKind.Calendar30E360,
                RateUnitOfTime = SimpleUnitOfTime.Y,
                Name = "Kamata"
            });
            request.InstallmentSchedule = new SimpleSchedule();
            request.InstallmentSchedule.DayOfMonth = 20;
            request.MinimumDaysForFirstInstallment = 20;
            request.NumberOfInstallments = 36;
            request.RepaymentType = RepaymentType.FixedAnnuity;
            request.InstallmentSchedule.FrequencyPeriod = 1;
            request.InstallmentSchedule.FrequencyUnitOfTime = SimpleUnitOfTime.M;
            request.IntercalarInterestRepaymentType = IntercalarInterestRepaymentType.WithFirstInstallment;

            request.Fees = new List<FeeEntry>();
            request.Fees.Add(new FeeEntry()
            {
                Percentage = (decimal)1.5,
                Date = request.StardDate,
                Kind = FeeConditionKind.OriginationFee,
                Frequency = FeeConditionFrequency.EventTriggered,
                CalculationBasisType = CalculationBasisType.ContractAmount,
                Name = "Naknada za obradu kreditnog zahteva"
            });

            request.AdjustFirstInstallment = true; // strange logic, but ok...

            var result = InstallmentPlanCalculation.CalculateInstallmentPlan(request);
            var someInr = result.Rows.Where(it => it.Date == new DateTime(2020, 3, 20)).First().InterestRepayment;
            var sumRepayment = result.Rows.Sum(it => it.PrincipalRepayment);
            Assert.True(sumRepayment == request.Amount);
            Assert.True(result.Rows.Count == 37);
            Assert.True(Math.Round(result.APR, 2) == (decimal)5.13);
            Assert.True(Math.Round(result.Annuity, 2) == (decimal)119.88);
            Assert.True(result.Rows[1].PrincipalRepayment == (decimal)106.38);
            Assert.True(result.Rows[1].InterestRepayment == (decimal)9.00);
            Assert.True(result.Rows.Last().PrincipalRepayment == (decimal)119.32);
            Assert.True(Math.Round(someInr, 2) == (decimal)12.08);
        }
        [Fact]
        public void TestSimpleInstallmentPlanCalculationCase3()
        {
            SimpleLoanCalculationRequest request = new SimpleLoanCalculationRequest();
            request.Amount = (decimal)519000;
            request.Currency = "RSD";
            request.StardDate = new DateTime(2019, 10, 15);
            request.RegularInterest = new List<InterestRateEntry>();
            request.RegularInterest.Add(new InterestRateEntry()
            {
                Date = request.StardDate,
                RatePercentage = 10.45,
                IsCompound = false,
                CalendarBasis = CalendarBasisKind.Calendar30E360,
                RateUnitOfTime = SimpleUnitOfTime.Y,
                Name = "Kamata"
            });
            request.InstallmentSchedule = new SimpleSchedule();
            request.InstallmentSchedule.DayOfMonth = 5;
            request.MinimumDaysForFirstInstallment = 20;
            request.NumberOfInstallments = 48;
            request.RepaymentType = RepaymentType.FixedAnnuity;
            request.InstallmentSchedule.FrequencyPeriod = 1;
            request.InstallmentSchedule.FrequencyUnitOfTime = SimpleUnitOfTime.M;
            request.IntercalarInterestRepaymentType = IntercalarInterestRepaymentType.WithFirstInstallment;

            request.Fees = new List<FeeEntry>();
            // Ne znam detalje pojedinacnih troskova, pa sam spojio u jedan fiksni iznos koji se vidi na amortplanu
            request.Fees.Add(new FeeEntry()
            {
                FixedAmount = 4967,
                Currency = "RSD",
                Date = request.StardDate,
                Kind = FeeConditionKind.OriginationFee,
                Frequency = FeeConditionFrequency.EventTriggered,
                CalculationBasisType = CalculationBasisType.ContractAmount,
                Name = "Naknada za obradu kreditnog zahteva, Kreditni biro, Menice"
            });

            request.AdjustFirstInstallment = true; // strange logic, but ok...

            var result = InstallmentPlanCalculation.CalculateInstallmentPlan(request);
            var someInr = result.Rows.Where(it => it.Date == new DateTime(2020, 3, 5)).First().InterestRepayment;
            var sumRepayment = result.Rows.Sum(it => it.PrincipalRepayment);
            Assert.True(sumRepayment == request.Amount);
            Assert.True(result.Rows.Count == 49);
            Assert.True(Math.Round(result.APR, 2) == (decimal)11.53);
            Assert.True(Math.Round(result.Annuity, 2) == (decimal)13275.63);
            Assert.True(result.Rows[1].PrincipalRepayment == (decimal)8756.01);
            Assert.True(result.Rows[1].InterestRepayment == (decimal)3013.08);
            Assert.True(result.Rows.Last().PrincipalRepayment == (decimal)13160.75);
            Assert.True(Math.Round(someInr, 2) == (decimal)4210.62);
        }
        [Fact]
        public void TestSimpleInstallmentPlanCalculationCase4()
        {
            SimpleLoanCalculationRequest request = new SimpleLoanCalculationRequest();
            request.Amount = (decimal)95500;
            request.Currency = "RSD";
            request.StardDate = new DateTime(2019, 10, 3);
            request.RegularInterest = new List<InterestRateEntry>();
            request.RegularInterest.Add(new InterestRateEntry()
            {
                Date = request.StardDate,
                RatePercentage = 15.950,
                IsCompound = false,
                CalendarBasis = CalendarBasisKind.Calendar30E360,
                RateUnitOfTime = SimpleUnitOfTime.Y,
                Name = "Kamata"
            });

            request.InstallmentSchedule = new SimpleSchedule();
            request.InstallmentSchedule.DayOfMonth = 5;
            request.MinimumDaysForFirstInstallment = 20;
            request.NumberOfInstallments = 60;
            request.RepaymentType = RepaymentType.FixedAnnuity;
            request.InstallmentSchedule.FrequencyPeriod = 1;
            request.InstallmentSchedule.FrequencyUnitOfTime = SimpleUnitOfTime.M;
            request.IntercalarInterestRepaymentType = IntercalarInterestRepaymentType.WithFirstInstallment;

            request.Fees = new List<FeeEntry>();
            // Ne znam detalje pojedinacnih troskova, pa sam spojio u jedan fiksni iznos koji se vidi na amortplanu
            request.Fees.Add(new FeeEntry()
            {
                FixedAmount = (decimal)2158.25,
                Currency = "RSD",
                Date = request.StardDate,
                Kind = FeeConditionKind.OriginationFee,
                Frequency = FeeConditionFrequency.EventTriggered,
                CalculationBasisType = CalculationBasisType.ContractAmount,
                Name = "Naknada za obradu kreditnog zahteva, Kreditni biro, Menice"
            });

            request.AdjustFirstInstallment = true; // strange logic, but ok...

            var result = InstallmentPlanCalculation.CalculateInstallmentPlan(request);
            var someInr = result.Rows.Where(it => it.Date == new DateTime(2020, 3, 5)).First().InterestRepayment;
            var sumRepayment = result.Rows.Sum(it => it.PrincipalRepayment);
            Assert.True(sumRepayment == request.Amount);
            Assert.True(result.Rows.Count == 61);
            Assert.True(Math.Round(result.APR, 2) == (decimal)18.39);
            Assert.True(Math.Round(result.Annuity, 2) == (decimal)2319.84);
            Assert.True(result.Rows[1].PrincipalRepayment == (decimal)1050.49);
            // Imamo razliku u zaokruzivanju
            // Assert.True(result.Rows[1].InterestRepayment == (decimal)1353.98);
            Assert.True(result.Rows[1].InterestRepayment == (decimal)1353.97);
            Assert.True(result.Rows.Last().PrincipalRepayment == (decimal)2289.23);
            Assert.True(Math.Round(someInr, 2) == (decimal)1212.38);
        }

        [Fact]
        public void TestCreditCardInstallmentPlan()
        {
            SimpleLoanCalculationRequest request = new SimpleLoanCalculationRequest();
            request.Amount = (decimal)100000;
            request.Currency = "RSD";
            request.StardDate = new DateTime(2019, 10, 3);
            request.RegularInterest = new List<InterestRateEntry>();
            request.RegularInterest.Add(new InterestRateEntry()
            {
                Date = request.StardDate,
                RatePercentage = 12,
                IsCompound = false,
                CalendarBasis = CalendarBasisKind.Calendar30E360,
                RateUnitOfTime = SimpleUnitOfTime.Y,
                Name = "Kamata"
            });
            request.InstallmentSchedule = new SimpleSchedule();
            request.InstallmentSchedule.DayOfMonth = 0;
            request.MinimumDaysForFirstInstallment = 0;
            request.NumberOfInstallments = 1;
            request.RepaymentType = RepaymentType.Bullet;
            request.InstallmentSchedule.FrequencyPeriod = 1;
            request.InstallmentSchedule.FrequencyUnitOfTime = SimpleUnitOfTime.Y;
            //request.IntercalarInterestRepaymentType = IntercalarInterestRepaymentType.WithFirstInstallment;

            request.InterestSchedule = new SimpleSchedule();
            request.InterestSchedule.DayOfMonth = 31;
            request.InterestSchedule.FrequencyPeriod = 1;
            request.InterestSchedule.FrequencyUnitOfTime = SimpleUnitOfTime.M;
            request.ForceInterestWithInstallment = true;

            request.Fees = new List<FeeEntry>();
            // Ne znam detalje pojedinacnih troskova, pa sam spojio u jedan fiksni iznos koji se vidi na amortplanu
            request.Fees.Add(new FeeEntry()
            {
                FixedAmount = (decimal)500,
                Currency = "RSD",
                Date = request.StardDate,
                Kind = FeeConditionKind.OriginationFee,
                Frequency = FeeConditionFrequency.EventTriggered,
                CalculationBasisType = CalculationBasisType.ContractAmount,
                Name = "Naknada za obradu kreditnog zahteva, Kreditni biro, Menice"
            });

            var result = InstallmentPlanCalculation.CalculateInstallmentPlan(request);
            var someInr = result.Rows.Where(it => it.Date == new DateTime(2020, 3, 31)).First().InterestRepayment;
            var sumRepayment = result.Rows.Sum(it => it.PrincipalRepayment);
            Assert.True(sumRepayment == request.Amount);
            Assert.True(result.Rows.Count == 14);
            //Assert.True(Math.Round(result.APR, 2) == (decimal)18.39);
            Assert.True(Math.Round(result.Annuity, 2) == (decimal)100000);
            Assert.True(result.Rows[1].PrincipalRepayment == (decimal)0);
            Assert.True(result.Rows[1].InterestRepayment == (decimal)900);
            Assert.True(result.Rows[2].InterestRepayment == (decimal)1000);
            Assert.True(result.Rows.Last().PrincipalRepayment == (decimal)100000);
            Assert.True(Math.Round(someInr, 2) == (decimal)1033.33);
        }
    }
}
