using EntityFrameworkCore.Triggered;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Threading;
using System;
using DotNetCore.CAP;
using static Grpc.Core.Metadata;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Services.Identity.API.Services
{
    public class CapUserEntity
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
    public class UserTriggers : IAfterSaveTrigger<IdentityUser>
    {
       

        private readonly ICapPublisher _capPublisher;
        private readonly ILogger _logger;

        public UserTriggers(ICapPublisher capPublisher, ILogger<UserTriggers> logger)
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
                    _capPublisher.Publish("UserAdded", new CapUserEntity() { Id = context.Entity.Id, UserName = context.Entity.UserName, Email = context.Entity.Email });
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
                    _capPublisher.Publish("UserDeleted", new CapUserEntity() { Id = context.Entity.Id, UserName = context.Entity.UserName, Email = context.Entity.Email });
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
                    _capPublisher.Publish("UserUpdated", new CapUserEntity() { Id = context.Entity.Id, UserName = context.Entity.UserName, Email = context.Entity.Email });
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
