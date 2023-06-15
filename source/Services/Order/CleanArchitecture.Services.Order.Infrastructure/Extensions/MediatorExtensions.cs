using CleanArchitecture.Services.Order.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Order.Infrastructure.Extensions
{
    public static class MediatorExtensions
    {
        public static async Task DispatchNotifications(this IMediator mediator, DbContext context)
        {
            var entities = context.ChangeTracker
                .Entries<BaseEntity>()
                .Where(e => e.Entity.Notifications.Any())
                .Select(e => e.Entity);

            var notifications = entities
                .SelectMany(e => e.Notifications)
                .ToList();

            entities.ToList().ForEach(e => e.ClearNotifications());

            foreach (var notification in notifications)
            {
                await mediator.Publish(notification);
            }
        }
    }
}
