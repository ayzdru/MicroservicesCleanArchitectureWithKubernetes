using CleanArchitecture.Services.Catalog.API.Services;
using System;

namespace CleanArchitecture.Services.Catalog.API.Interfaces
{
    public interface ISubscriberService
    {
        void UserAdded(CapUserEntity capUserEntity);
        void UserDeleted(CapUserEntity capUserEntity);
        void UserUpdated(CapUserEntity capUserEntity);
    }
}
