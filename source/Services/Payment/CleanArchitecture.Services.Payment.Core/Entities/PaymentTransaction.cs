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
        public Money Amount { get; set; }
        public PaymentTransaction(Guid paymentId)
        {
            PaymentId = paymentId;
        }
        public PaymentTransaction(Guid paymentId, Money amount)
        {
            PaymentId = paymentId;
            Amount = amount;
        }
    }
}
