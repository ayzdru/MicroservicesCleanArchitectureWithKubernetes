using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Payment.Core.Entities
{
    public class Payment : BaseEntity
    {
        public Order Order { get; private set; }
        public Guid OrderId { get; private set; }      

        public Payment(Guid id, Guid orderId)
        {
            Id = id;
            OrderId = orderId;
        }
        private readonly List<PaymentTransaction> _paymentTransactions = new List<PaymentTransaction>();
        public IReadOnlyCollection<PaymentTransaction> PaymentTransactions => _paymentTransactions.AsReadOnly();
    }
}
