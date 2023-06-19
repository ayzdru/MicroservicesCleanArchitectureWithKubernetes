using CleanArchitecture.Services.Order.Core.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Order.Application.Notifications
{
    public class CheckoutOrderNotification : INotification
    {
        public class CheckoutOrderNotificationHandler : INotificationHandler<CheckoutOrderNotification>
        {
            public Task Handle(CheckoutOrderNotification notification, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}
