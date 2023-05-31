using CleanArchitecture.Services.Order.API.Services;
using System;

namespace CleanArchitecture.Services.Order.API.Interfaces
{
    public interface ISubscriberService
    {
        void UserAdded(CapUserEntity capUserEntity);
        void UserDeleted(CapUserEntity capUserEntity);
        void UserUpdated(CapUserEntity capUserEntity);
    }
}
