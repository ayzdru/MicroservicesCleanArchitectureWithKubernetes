using CleanArchitecture.Services.Payment.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Payment.Core.Entities
{
    public class Order : BaseEntity
    {
        public Money TotalAmount { get; set; }
        public Order(Guid id, Money totalAmount)
        {
            Id = id;
            TotalAmount = totalAmount;
        }
        public Order(Guid id)
        {
            Id = id;
        }
    }
}
