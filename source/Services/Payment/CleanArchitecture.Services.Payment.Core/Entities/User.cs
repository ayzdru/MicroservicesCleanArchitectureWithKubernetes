using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Payment.Core.Entities
{
    public class User : BaseEntity
    {
        public string UserName { get; private set; }
        public string Email { get; private set; }
        public User(Guid id, string userName, string email)
        {
            Id = id;
            UserName = userName;
            Email = email;
        }

    }
}
