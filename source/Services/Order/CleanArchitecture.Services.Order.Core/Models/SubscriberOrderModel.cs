using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Order.Core.Models
{
    public class SubscriberOrderModel
    {
        public Guid OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string CurrencyName { get; }
        public string CurrencySymbol { get; }
    }
}
