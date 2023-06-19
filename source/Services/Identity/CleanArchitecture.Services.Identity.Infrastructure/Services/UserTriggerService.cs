using EntityFrameworkCore.Triggered;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Threading;
using System;
using DotNetCore.CAP;
using Microsoft.Extensions.Logging;
using CleanArchitecture.Services.Identity.Core.Models;

namespace CleanArchitecture.Services.Identity.Infrastructure.Services
{
    public class UserTriggerService : IAfterSaveTrigger<IdentityUser>
    {
       

        private readonly ICapPublisher _capPublisher;
        private readonly ILogger _logger;

        public UserTriggerService(ICapPublisher capPublisher, ILogger<UserTriggerService> logger)
        {
            _capPublisher = capPublisher ?? throw new ArgumentNullException(nameof(capPublisher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public Task AfterSave(ITriggerContext<IdentityUser> context, CancellationToken cancellationToken)
        {
            if (context.ChangeType == ChangeType.Added)
            {
                try
                {
                    _capPublisher.Publish("UserAdded", new SubscriberUserModel() { Id = context.Entity.Id, UserName = context.Entity.UserName, Email = context.Entity.Email });
                    _logger.LogInformation("UserAdded", context.Entity.Id, context.Entity.UserName, context.Entity.Email);
                }
                catch (Exception ex)
                {

                    _logger.LogError(ex, "UserAdded");
                }
            }
            else if (context.ChangeType == ChangeType.Deleted)
            {
                try
                {
                    _capPublisher.Publish("UserDeleted", new SubscriberUserModel() { Id = context.Entity.Id, UserName = context.Entity.UserName, Email = context.Entity.Email });
                    _logger.LogInformation("UserDeleted", context.Entity.Id, context.Entity.UserName, context.Entity.Email);
                }
                catch (Exception ex)
                {

                    _logger.LogError(ex, "UserDeleted");
                }
            }
            else if (context.ChangeType == ChangeType.Modified)
            {
                try
                {
                    _capPublisher.Publish("UserUpdated", new SubscriberUserModel() { Id = context.Entity.Id, UserName = context.Entity.UserName, Email = context.Entity.Email });
                    _logger.LogInformation("UserUpdated", context.Entity.Id, context.Entity.UserName, context.Entity.Email);
                }
                catch (Exception ex)
                {

                    _logger.LogError(ex, "UserUpdated");
                }
            }
            return Task.CompletedTask;
        }
    }
}
