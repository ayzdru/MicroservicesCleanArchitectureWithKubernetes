using CleanArchitecture.Services.Payment.Core.Entities;
using CleanArchitecture.Services.Payment.Core.Models;
using System;

namespace CleanArchitecture.Services.Payment.Core.Interfaces
{
    public interface ISubscriberService
    {
        Task UserAdded(SubscriberUserModel  subscriberUserModel);
        Task UserDeleted(SubscriberUserModel subscriberUserModel);
        Task UserUpdated(SubscriberUserModel subscriberUserModel);
        Task OrderAdded(SubscriberOrderModel subscriberOrderModel);
    }
}
