using CleanArchitecture.Services.Payment.API.Data;
using CleanArchitecture.Services.Payment.API.Interfaces;
using DotNetCore.CAP;
using System;
using System.Linq;

namespace CleanArchitecture.Services.Payment.API.Services
{

    public class CapUserEntity
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
    public class SubscriberService : ISubscriberService, ICapSubscribe
    {
        private readonly PaymentDbContext _paymentDbContext;
        public SubscriberService(PaymentDbContext paymentDbContext)
        {
            _paymentDbContext = paymentDbContext;
        }
        [CapSubscribe("UserAdded")]
        public void UserAdded(CapUserEntity capUserEntity)
        {
            _paymentDbContext.Users.Add(new Entities.User { Id = Guid.Parse(capUserEntity.Id), UserName = capUserEntity.UserName, Email = capUserEntity.Email });
            _paymentDbContext.SaveChanges();
        }
        [CapSubscribe("UserDeleted")]
        public void UserDeleted(CapUserEntity capUserEntity)
        {
            var user = _paymentDbContext.Users.Where(q => q.Id == Guid.Parse(capUserEntity.Id)).SingleOrDefault();
            if (user != null)
            {
                _paymentDbContext.Users.Remove(user);
                _paymentDbContext.SaveChanges();
            }
            else
            {
                throw new ArgumentNullException(nameof(user));
            }
        }
        [CapSubscribe("UserUpdated")]
        public void UserUpdated(CapUserEntity capUserEntity)
        {
            var user = _paymentDbContext.Users.Where(q => q.Id == Guid.Parse(capUserEntity.Id)).SingleOrDefault();
            if (user != null)
            {
                user.UserName = capUserEntity.UserName;
                user.Email = capUserEntity.Email;
                _paymentDbContext.Users.Update(user);
                _paymentDbContext.SaveChanges();
            }
            else
            {
                throw new ArgumentNullException(nameof(user));
            }
        }
    }
}
