using CleanArchitecture.Services.Order.API.Data;
using CleanArchitecture.Services.Order.API.Interfaces;
using DotNetCore.CAP;
using System;
using System.Linq;

namespace CleanArchitecture.Services.Order.API.Services
{

    public class CapUserEntity
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
    public class SubscriberService : ISubscriberService, ICapSubscribe
    {
        private readonly OrderDbContext _orderDbContext;
        public SubscriberService(OrderDbContext orderDbContext)
        {
            _orderDbContext = orderDbContext;
        }
        [CapSubscribe("UserAdded")]
        public void UserAdded(CapUserEntity capUserEntity)
        {
            _orderDbContext.Users.Add(new Entities.User { Id = Guid.Parse(capUserEntity.Id), UserName = capUserEntity.UserName, Email = capUserEntity.Email });
            _orderDbContext.SaveChanges();
        }
        [CapSubscribe("UserDeleted")]
        public void UserDeleted(CapUserEntity capUserEntity)
        {
            var user = _orderDbContext.Users.Where(q => q.Id == Guid.Parse(capUserEntity.Id)).SingleOrDefault();
            if (user != null)
            {
                _orderDbContext.Users.Remove(user);
                _orderDbContext.SaveChanges();
            }
            else
            {
                throw new ArgumentNullException(nameof(user));
            }
        }
        [CapSubscribe("UserUpdated")]
        public void UserUpdated(CapUserEntity capUserEntity)
        {
            var user = _orderDbContext.Users.Where(q => q.Id == Guid.Parse(capUserEntity.Id)).SingleOrDefault();
            if (user != null)
            {
                user.UserName = capUserEntity.UserName;
                user.Email = capUserEntity.Email;
                _orderDbContext.Users.Update(user);
                _orderDbContext.SaveChanges();
            }
            else
            {
                throw new ArgumentNullException(nameof(user));
            }
        }
    }
}
