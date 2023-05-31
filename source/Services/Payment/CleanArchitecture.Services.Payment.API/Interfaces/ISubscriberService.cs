using CleanArchitecture.Services.Payment.API.Services;
using System;

namespace CleanArchitecture.Services.Payment.API.Interfaces
{
    public interface ISubscriberService
    {
        void UserAdded(CapUserEntity capUserEntity);
        void UserDeleted(CapUserEntity capUserEntity);
        void UserUpdated(CapUserEntity capUserEntity);
    }
}
