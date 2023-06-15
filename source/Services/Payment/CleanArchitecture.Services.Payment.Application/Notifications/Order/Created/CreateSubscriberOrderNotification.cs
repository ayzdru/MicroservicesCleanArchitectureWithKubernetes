using CleanArchitecture.Services.Payment.Core.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Payment.Application.Notifications
{
    public class CreateSubscriberOrderNotification : INotification
    {
        public SubscriberOrderModel SubscriberOrder { get; set; }
        public class TodoListCreatedNotificationHandler : INotificationHandler<CreateSubscriberOrderNotification>
        {
            public Task Handle(CreateSubscriberOrderNotification notification, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}
