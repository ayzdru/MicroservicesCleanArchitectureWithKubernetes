using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Identity.Application.Models
{
    public record BasketItemModel
    {
        public Guid ProductId { get; init; }
        public string Name { get; init; }
        public string Description { get; init; }
        public int Quantity { get; init; }
        public decimal Price { get; init; }
        public BasketItemModel(Guid productId, string name, string description, int quantity, decimal price)
        {
            ProductId = productId;
            Name = name;
            Description = description;
            Quantity = quantity;
            Price = price;
        }
       
    }
}
