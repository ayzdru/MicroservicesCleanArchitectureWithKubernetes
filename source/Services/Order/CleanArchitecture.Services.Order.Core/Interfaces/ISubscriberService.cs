using CleanArchitecture.Services.Order.Core.Entities;
using CleanArchitecture.Services.Order.Core.Models;
using System;

namespace CleanArchitecture.Services.Order.Core.Interfaces
{
    public interface ISubscriberService
    {
        Task UserAdded(SubscriberUserModel  subscriberUserModel);
        Task UserDeleted(SubscriberUserModel subscriberUserModel);
        Task UserUpdated(SubscriberUserModel subscriberUserModel);
        Task OrderAdded(SubscriberOrderModel subscriberOrderModel);
    }
}
