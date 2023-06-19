using CleanArchitecture.Services.Order.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Order.Core.Entities
{
    public class OrderItem : BaseEntity
    {
        public Order Order { get; private set; }
        public Guid OrderId { get; private set; }
        public Product Product { get; private set; }
        public Guid ProductId { get; private set; }
        public int Quantity { get; private set; }
        public Money TotalAmount { get; private set; }
        public OrderItem(Guid orderId, Guid productId, int quantity, Money totalAmount)
        {        
            OrderId = orderId;
            ProductId = productId;
            Quantity = quantity;
            TotalAmount = totalAmount;
        }      
    }
}
