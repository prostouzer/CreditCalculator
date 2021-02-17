using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Foolproof;

namespace CreditCalculator.Models
{
    public class Credit
    {
        public const int MinAmount = 1000; // минимально возможный кредит
        public const int MinRate = 1; // минимально возможная ставка
        public const int MaxRate = 10000; // максимальная ставка % годовых
        public const int MaxPaymentPeriodsCount = 1000; // максимальное количество платежей

        [Display(Name = "Сумма кредита")]
        [Required(ErrorMessage = "Введите сумму кредита")]
        [Range(MinAmount, int.MaxValue, ErrorMessage = "Неправильное значение")]
        public decimal Amount { get; set; }

        [Display(Name = "Дата начала")]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Укажите дату начала")]
        [LessThan("EndDate", ErrorMessage = "Дата начала должна быть раньше даты окончания")]
        public DateTime BeginDate { get; set; }

        [Display(Name = "Дата окончания")]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Укажите дату окончания")]
        [GreaterThan("BeginDate", ErrorMessage = "Дата окончания должна быть позже даты начала")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Ставка, % годовых")]
        [Required(ErrorMessage = "Укажите ставку")]
        [Range(1, MaxRate, ErrorMessage = "Неправильное значение")]
        public double Rate { get; set; }

        [Display(Name = "Количество платежей")]
        [Required(ErrorMessage = "Укажите количество платежей")]
        [Range(MinRate, MaxPaymentPeriodsCount, ErrorMessage = "Неправильное значение")]
        public int PaymentPeriodsCount { get; set; }

        public decimal TotalPayment { get; set; }
        public decimal Overpay { get; set; }
        public List<Payment> PaymentsList { get; set; } = new List<Payment>();

        public void Configure()
        {
            Rate *= 0.01; 
            Rate /= 12; // проценты годовых
            TotalPayment = CalculateTotalPayment();
            Overpay = CalculateOverpay();
            GetPaymentsList();
        }

        public decimal CalculateTotalPayment()
        {
            var annuityPayment = CalculateAnnuityPayment(Amount);
            return annuityPayment * PaymentPeriodsCount;
        }

        public decimal CalculateOverpay()
        {
            return CalculateTotalPayment() - Amount;
        }

        public void GetPaymentsList()
        {
            var paymentDate = BeginDate;
            var daysBetweenPeriods = GetDaysBetweenPeriods(); // количество дней между платежами
            var annuityPayment = CalculateAnnuityPayment(Amount);
            var remainingDebt = Amount;
            for (var i = 0; i < PaymentPeriodsCount; i++)
            {
                var number = i + 1;
                var body = CalculateBody(number, annuityPayment);
                var percent = CalculatePercent(number, annuityPayment);
                paymentDate = paymentDate.AddDays(daysBetweenPeriods);
                PaymentsList.Add(new Payment(number, paymentDate, annuityPayment, body, percent, remainingDebt -= body));
            }
        }

        public decimal CalculateAnnuityPayment(decimal creditAmount)
        {
            // коэффициент аннуитета = (i*(1+i)^n)/((1+i)^n - 1), где i - процентная ставка по кредиту, n = количество платежей
            var annuityCoefficient = Rate * Math.Pow(1 + Rate, PaymentPeriodsCount) / (Math.Pow(1 + Rate, PaymentPeriodsCount) - 1);

            // размер аннуитетного платежа = коэффициент аннуитета * сумма кредита
            var annuityPayment = (decimal)annuityCoefficient * creditAmount;
            return annuityPayment;
        }
        public decimal CalculateBody(int number, decimal annuityPayment)
        {
            return annuityPayment / (decimal)Math.Pow(1 + Rate, PaymentPeriodsCount - number + 1);
        }
        public decimal CalculatePercent(int number, decimal annuityPayment)
        {
            return annuityPayment * (1 - 1 / (decimal)Math.Pow(1 + Rate, PaymentPeriodsCount - number + 1));
        }

        public int GetTotalDays()
        {
            return (EndDate.Date - BeginDate.Date).Days;
        }

        public int GetDaysBetweenPeriods()
        {
            return GetTotalDays() / PaymentPeriodsCount;
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
}