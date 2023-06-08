using CleanArchitecture.Services.Catalog.Core.Models;
using System;

namespace CleanArchitecture.Services.Catalog.Core.Interfaces
{
    public interface ISubscriberService
    {
        Task UserAdded(SubscriberUserModel  subscriberUserModel);
        Task UserDeleted(SubscriberUserModel subscriberUserModel);
        Task UserUpdated(SubscriberUserModel subscriberUserModel);
    }
}
