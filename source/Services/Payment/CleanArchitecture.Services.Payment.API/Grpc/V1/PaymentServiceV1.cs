using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Services.Payment.API.Data;
using DotNetCore.CAP;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace CleanArchitecture.Services.Payment.API.Grpc.V1
{
    public class PaymentServiceV1 : Payment.PaymentBase, ICapSubscribe
    {
        private readonly PaymentDbContext _paymentDbContext;
        private readonly ICapPublisher _capBus;
        public PaymentServiceV1(PaymentDbContext paymentDbContext, ICapPublisher capBus)
        {
            _paymentDbContext = paymentDbContext;
            _capBus = capBus;
        }
        [CapSubscribe("AddPayment")]
        public void AddPayment(Guid orderId, [FromCap] CapHeader header)
        {
            using (var transaction = _paymentDbContext.Database.BeginTransaction(_capBus, autoCommit: false))
            {
                _paymentDbContext.Payments.Add(new Entities.Payment() { OrderId = orderId });
                _paymentDbContext.SaveChanges();
                transaction.Commit();
            }
        }

    }
}
