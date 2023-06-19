using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CleanArchitecture.Services.Identity.Core
{
    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; }
        public IdentityUser CreatedByUser { get; protected set; }
        public Guid? CreatedByUserId { get; protected set; }
        public DateTime Created { get; protected set; }
        public IdentityUser LastModifiedByUser { get; protected set; }
        public Guid? LastModifiedByUserId { get; protected set; }
        public DateTime? LastModified { get; protected set; }
        public byte[] RowVersion { get; protected set; }
        public void Create(Guid? createdByUserId)
        {
            CreatedByUserId = createdByUserId;
            Created = DateTime.Now;
        }
        public void Update(Guid? lastModifiedByUserId)
        {
            if (lastModifiedByUserId.HasValue)
            {
                LastModifiedByUserId = lastModifiedByUserId;
                LastModified = DateTime.Now;
            }
        }
        private readonly List<BaseNotification> _notifications = new();

        [NotMapped]
        public IReadOnlyCollection<BaseNotification> Notifications => _notifications.AsReadOnly();

        public void AddNotification(BaseNotification baseNotification)
        {
            _notifications.Add(baseNotification);
        }

        public void RemoveNotification(BaseNotification baseNotification)
        {
            _notifications.Remove(baseNotification);
        }

        public void ClearNotifications()
        {
            _notifications.Clear();
        }
    }
}
