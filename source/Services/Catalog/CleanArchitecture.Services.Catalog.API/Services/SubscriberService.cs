using CleanArchitecture.Services.Catalog.API.Data;
using CleanArchitecture.Services.Catalog.API.Interfaces;
using CleanArchitecture.Shared.DataProtection.Redis;
using DotNetCore.CAP;
using System;
using System.Linq;

namespace CleanArchitecture.Services.Catalog.API.Services
{

    public class CapUserEntity
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
    public class SubscriberService : ISubscriberService, ICapSubscribe
    {      
        private readonly CatalogDbContext _catalogDbContext;
        public SubscriberService(CatalogDbContext catalogDbContext)
        {
            _catalogDbContext = catalogDbContext;
        }
        [CapSubscribe("UserAdded")]
        public void UserAdded(CapUserEntity capUserEntity)
        {
            _catalogDbContext.Users.Add(new Entities.User { Id = Guid.Parse(capUserEntity.Id), UserName = capUserEntity.UserName, Email = capUserEntity.Email });
            _catalogDbContext.SaveChanges();
        }
        [CapSubscribe("UserDeleted")]
        public void UserDeleted(CapUserEntity capUserEntity)
        {
            var user = _catalogDbContext.Users.Where(q => q.Id == Guid.Parse(capUserEntity.Id)).SingleOrDefault();
            if (user != null)
            {
                _catalogDbContext.Users.Remove(user);
                _catalogDbContext.SaveChanges();
            }
            else
            {
                throw new ArgumentNullException(nameof(user));
            }
        }
        [CapSubscribe("UserUpdated")]
        public void UserUpdated(CapUserEntity capUserEntity)
        {
            var user = _catalogDbContext.Users.Where(q => q.Id == Guid.Parse(capUserEntity.Id)).SingleOrDefault();
            if (user != null)
            {
                user.UserName = capUserEntity.UserName;
                user.Email = capUserEntity.Email;
                _catalogDbContext.Users.Update(user);
                _catalogDbContext.SaveChanges();
            }
            else
            {
                throw new ArgumentNullException(nameof(user));
            }
        }
    }
}
