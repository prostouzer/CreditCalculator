using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;

namespace CreditCalculator.Models
{
    public class Credit
    {
        public const decimal MinAmount = 1000; // минимально возможный кредит
        public const decimal MinRate = 1; // минимально возможная ставка

        public decimal Amount { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public double Rate { get; set; }

        public int PaymentPeriodsCount { get; set; } // количество платежей
        public decimal TotalPayment { get; set; }
        public List<Payment> PaymentsList { get; set; }

        public Credit(decimal amount, DateTime beginDate, DateTime endDate, decimal rate, int paymentPeriodsNumber)
        {
            // валидация
            if (!((amount >= MinAmount) && (beginDate < endDate) && rate >= MinRate) && (paymentPeriodsNumber >= 1))
                throw new ArgumentException($"Ошибка во входных параметрах (сумма должна быть >= {MinAmount}, дата начала должна быть меньше даты окончания, ставка должна быть >= {MinRate}, количество платежей должно быть >= {PaymentPeriodsCount}");

            TotalPayment = CalculateTotalPayment();
            GetPaymentsList();
        }

        public decimal CalculateTotalPayment()
        {
            var annuityPayment = CalculateAnnuityPayment(Amount);
            return annuityPayment * PaymentPeriodsCount;
        }

        public void GetPaymentsList()
        {
            var paymentDate = BeginDate;
            var daysBetweenPeriods = GetDaysBetweenPeriods(); // количество дней между платежами
            var annuityPayment = CalculateAnnuityPayment(Amount);
            for (int i = 0; i < PaymentPeriodsCount; i++)
            {
                PaymentsList.Add(new Payment(i + 1, paymentDate.AddDays(daysBetweenPeriods), annuityPayment, TotalPayment - annuityPayment));
            }
        }

        public decimal CalculateAnnuityPayment(decimal creditAmount)
        {
            // коэффициент аннуитета = (i*(1+i)^n)/((1+i)^n - 1), где i - процентная ставка по кредиту, n = количество платежей
            var annuityCoefficient = (Rate * Math.Pow((1 + Rate), PaymentPeriodsCount)) / (Math.Pow((1 + Rate), PaymentPeriodsCount) - 1);

            // размер аннуитетного платежа = коэффициент аннуитета * сумма кредита
            var annuityPayment = (decimal)annuityCoefficient * creditAmount;
            return annuityPayment;
        }

        //public void GetPaymentsList()
        //{
        //    var paymentDate = BeginDate;
        //    var daysBetweenPeriods = GetDaysBetweenPeriods();
        //    List<Payment> payments = new List<Payment>();
        //    for (int i = 0; i<PaymentPeriodsCount.)

        //    var payment = new Payment(i + 1, paymentDate.AddDays(daysBetweenPeriods), 1, 1, )
        //}

        public int GetTotalDays()
        {
            return (BeginDate.Date - EndDate.Date).Days;
        }

        public int GetDaysBetweenPeriods()
        {
            return GetTotalDays() / PaymentPeriodsCount;
        }

        public class Payment // для отображения списка платежей на таблице
        {
            public int Number { get; set; } // № платежа
            public DateTime Date { get; set; } // дата платежа
            public decimal Body { get; set; } // размер платежа по телу
            public decimal Percent { get; set; } // размер платежа по проценту
            public decimal RemainingDebt { get; set; } // остаток основного долга

            // TODO get percent/body payment amounts
            //public Payment(int number, DateTime date, decimal body, decimal percent, decimal remainingDebt)
            public Payment(int number, DateTime date, decimal payment, decimal remainingDebt)
            {
                Number = number;
                Date = date.Date;
                Body = payment;
                Percent = payment;
                //Body = body;
                //Percent = percent;
                RemainingDebt = remainingDebt;
            }
        }
    }
}