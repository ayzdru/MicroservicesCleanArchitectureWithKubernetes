﻿using CleanArchitecture.Services.Catalog.Core.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Catalog.Application.Notifications
{
    public class CreateSubscriberUserNotification : INotification
    {
        public SubscriberUserModel SubscriberUser { get; set; }
        public class TodoListCreatedNotificationHandler : INotificationHandler<CreateSubscriberUserNotification>
        {
            public Task Handle(CreateSubscriberUserNotification notification, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}
