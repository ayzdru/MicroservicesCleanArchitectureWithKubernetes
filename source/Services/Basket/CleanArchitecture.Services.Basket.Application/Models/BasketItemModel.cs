using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Basket.Application.Models
{
    public record BasketItemModel
    {
        public BasketItemModel(string productId, int quantity)
        {
            ProductId = productId;
            Quantity = quantity;
        }

        public string ProductId { get; init; }
        public int Quantity { get; init; }
    }
}
