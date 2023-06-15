using CleanArchitecture.Services.Order.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Order.Core.Entities
{
    public class Order : BaseEntity
    {
        public Money TotalAmount { get; private set; }
        public Order(Guid id, Money totalAmount)
        {
            Id = id;
            TotalAmount = totalAmount;
        }
      
    }
}
