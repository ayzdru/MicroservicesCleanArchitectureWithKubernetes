using CleanArchitecture.Services.Order.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CleanArchitecture.Services.Order.Core.Entities
{
    public class Order : BaseEntity
    {
        public Money TotalAmount { get; set; }
        private readonly List<OrderItem> _orderItems = new List<OrderItem>();
        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();
        public Order(Guid id, Money totalAmount)
        {
            Id = id;
            TotalAmount = totalAmount;
        }
        public Order(Guid id)
        {
            Id = id;
        }
        public void AddOrderItem(OrderItem orderItem)
        {
            _orderItems.Add(orderItem);
        }
    }
}
