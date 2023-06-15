using CleanArchitecture.Services.Payment.Core;
using CleanArchitecture.Services.Payment.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Payment.Core.Entities
{
    public class PaymentTransaction : BaseEntity
    {       
        public Payment Payment { get; private set; }
        public Guid PaymentId { get; private set; }
        public Money Amount { get; private set; }
        public PaymentTransaction(Payment payment, Guid paymentId, Money amount)
        {
            Payment = payment;
            PaymentId = paymentId;
            Amount = amount;
        }
    }
}
