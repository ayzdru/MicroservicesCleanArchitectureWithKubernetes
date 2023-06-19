﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Payment.Core.Models
{
    public class SubscriberOrderModel
    {
        public Guid OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; }
    }
}
