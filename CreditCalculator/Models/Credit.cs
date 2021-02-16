using System;
using System.Collections.Generic;
using System.Linq;
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
        public decimal Rate { get; set; }

        public decimal TotalPayment { get; set; }
        public List<Payment> PaymentsList { get; set; }

        public Credit(decimal amount, DateTime beginDate, DateTime endDate, decimal rate)
        {
            // валидация
            if (!((amount >= MinAmount) && (beginDate < endDate)&& rate >= MinRate))
                throw new ArgumentException($"Ошибка во входных параметрах (сумма должна быть >= {MinAmount}, дата начала должна быть меньше даты окончания, ставка должна быть >= {MinRate}");

            TotalPayment = CalculateTotalPayment();
        }

        public decimal CalculateTotalPayment()
        {
            return 0;
        }

        public int CountDays()
        {
            return (BeginDate.Date - EndDate.Date).Days;
        }

        public class Payment // для отображения списка платежей на таблице
        {
            public int Number { get; set; } // № платежа
            public DateTime Date { get; set; }
            public decimal BodyAmount { get; set; }
            public decimal PercentAmount { get; set; }
            public decimal RemainingDebt { get; set; }

            public Payment(int number, DateTime date, decimal bodyAmount, decimal percentAmount, decimal remainingDebt)
            {
                Number = number;
                Date = date;
                BodyAmount = bodyAmount;
                PercentAmount = percentAmount;
                RemainingDebt = remainingDebt;
            }
        }
    }
}