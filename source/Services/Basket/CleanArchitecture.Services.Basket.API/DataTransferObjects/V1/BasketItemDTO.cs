using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Basket.API.DataTransferObjects.V1
{
    public class BasketItemDTO
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
