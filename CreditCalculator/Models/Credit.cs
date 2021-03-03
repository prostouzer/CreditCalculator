using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Foolproof;

namespace CreditCalculator.Models
{
    public class Credit
    {
        public const int MinAmount = 1000; // минимально возможная сумма кредита

        public const int MinRate = 1; // минимально возможная ставка % годовых
        public const int MaxRate = 10000; // максимальная ставка % годовых

        public const int MinDays = 2; // минимальное количество дней
        public const int MaxDays = 1000; // максимальное количество дней

        public const int MinMonths = 1; // минимальное количество месяцев
        public const int MaxMonths = 1200; // максимальное количество месяцев

        private DateTime _beginDate;

        private int _paymentPeriodsCount;


        [Display(Name = "Сумма кредита")]
        [Required(ErrorMessage = "Введите сумму кредита")]
        [Range(MinAmount, int.MaxValue, ErrorMessage = "Неправильное значение")]
        public decimal CreditAmount { get; set; }

        [Display(Name = "Ставка, % в день")]
        [Required(ErrorMessage = "Укажите ставку")]
        [Range(MinRate, MaxRate, ErrorMessage = "Неправильное значение")]
        public double Rate { get; set; }

        [Display(Name = "Годовая ставка")]
        public bool IsAnnualRate { get; set; } // годовая ставка

        [Display(Name = "Периодичность платежей")]
        [Required(ErrorMessage = "Укажите периодичность платежей")]
        public RepaymentPeriodicity RepaymentPeriodicity { get; set; } = RepaymentPeriodicity.Months;

        [Display(Name = "Срок (дней)")]
        [RequiredIf("RepaymentPeriodicity", Operator.EqualTo, RepaymentPeriodicity.Days, ErrorMessage = "Введите значение")]
        [Range(MinDays, MaxDays, ErrorMessage = "Неправильное значение")]
        public int? Days { get; set; }

        [Display(Name = "Шаг платежа в днях")]
        [RequiredIf("RepaymentPeriodicity", Operator.EqualTo, RepaymentPeriodicity.Days, ErrorMessage = "Введите значение")]
        [Range(1, MaxDays)] // количество дней между платежами должно быть больше 1 и не больше MaxDays 
        [LessThanOrEqualTo("Days", ErrorMessage = "Шаг платежа не может быть больше срока")]
        public int? DaysBetweenPeriods { get; set; }

        [Display(Name = "Срок (месяцев)")]
        [RequiredIf("RepaymentPeriodicity", Operator.EqualTo, RepaymentPeriodicity.Months, ErrorMessage = "Введите значение")]
        [Range(MinMonths, MaxMonths, ErrorMessage = "Неправильное значение")]
        public int? Months { get; set; }

        public decimal TotalPayment { get; set; }
        public decimal Overpay { get; set; }
        public List<Payment> PaymentsList { get; set; } = new List<Payment>();

        public void Configure()
        {
            Rate *= 0.01;
            if (IsAnnualRate)
            {
                Rate /= 12;
            }
            else
            {
                var coef = (double)365 / 12;
                Rate *= coef;
            }
            _paymentPeriodsCount = CalculatePeriodsCount();

            TotalPayment = CalculateTotalPayment();
            Overpay = CalculateOverpay();
            _beginDate = DateTime.Now.Date;
            GetPaymentsList();
        }

        public int CalculatePeriodsCount()
        {
            switch (RepaymentPeriodicity)
            {
                case RepaymentPeriodicity.Days:
                    return GetPeriodsCount_Days();
                case RepaymentPeriodicity.Months:
                    return (GetPeriodsCount_Months()); // оплата каждый месяц
            }

            return -1;
        }
        public int GetPeriodsCount_Days()
        {
            var totalDays = Days ?? default(int);
            var daysBetweenPeriods = DaysBetweenPeriods ?? default(int);
            return totalDays / daysBetweenPeriods;
        }

        public int GetPeriodsCount_Months()
        {
            return Months ?? default(int); // оплата каждый месяц
        }

        public decimal CalculateTotalPayment()
        {
            var annuityPayment = CalculateAnnuityPayment(CreditAmount);
            return annuityPayment * _paymentPeriodsCount;
        }

        public decimal CalculateOverpay()
        {
            return CalculateTotalPayment() - CreditAmount;
        }

        public void GetPaymentsList()
        {
            var paymentDate = _beginDate;
            var annuityPayment = CalculateAnnuityPayment(CreditAmount);
            var remainingDebt = CreditAmount;
            for (var i = 0; i < _paymentPeriodsCount; i++)
            {
                var number = i + 1;
                var body = CalculateBody(number, annuityPayment);
                var percent = CalculatePercent(number, annuityPayment);

                paymentDate = AddTimeToPaymentDate(paymentDate);
                PaymentsList.Add(new Payment(number, paymentDate, annuityPayment, body, percent, remainingDebt -= body));
            }
        }

        public DateTime AddTimeToPaymentDate(DateTime paymentDate) // расчитывает дату оплаты очередного платежа
        {
            switch (RepaymentPeriodicity)
            {
                case RepaymentPeriodicity.Days:
                    var daysBetweenPeriods = DaysBetweenPeriods ?? default(int);
                    return paymentDate.AddDays(daysBetweenPeriods);
                case RepaymentPeriodicity.Months:
                    return paymentDate.AddMonths(1); // оплата ежемесячно
            }

            return default(DateTime);
        }

        public decimal CalculateAnnuityPayment(decimal creditAmount)
        {
            // коэффициент аннуитета = (i*(1+i)^n)/((1+i)^n - 1), где i - процентная ставка по кредиту, n = количество платежей
            var annuityCoefficient = Rate * Math.Pow(1 + Rate, _paymentPeriodsCount) / (Math.Pow(1 + Rate, _paymentPeriodsCount) - 1);

            // размер аннуитетного платежа = коэффициент аннуитета * сумма кредита
            var annuityPayment = (decimal)annuityCoefficient * creditAmount;
            return annuityPayment;
        }

        public decimal CalculateBody(int number, decimal annuityPayment)
        {
            return annuityPayment / (decimal)Math.Pow(1 + Rate, _paymentPeriodsCount - number + 1);
        }

        public decimal CalculatePercent(int number, decimal annuityPayment)
        {
            return annuityPayment * (1 - 1 / (decimal)Math.Pow(1 + Rate, _paymentPeriodsCount - number + 1));
        }

        public class Payment // для отображения списка платежей на таблице
        {
            public int Number { get; set; } // № платежа
            public DateTime Date { get; set; } // дата платежа
            public decimal AnnuityPayment { get; set; }
            public decimal Body { get; set; } // размер платежа по телу
            public decimal Percent { get; set; } // размер платежа по проценту
            public decimal RemainingDebt { get; set; } // остаток основного долга

            public Payment(int number, DateTime date, decimal payment, decimal body, decimal percent, decimal remainingDebt)
            {
                Number = number;
                Date = date.Date;
                AnnuityPayment = payment;
                Body = body;
                Percent = percent;
                RemainingDebt = remainingDebt;
            }
        }
    }
    public enum RepaymentPeriodicity
    {
        [Display(Name = "В днях")]
        Days = 0,

        [Display(Name = "В месяцах")]
        Months = 1
    }
}