using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Catalog.Core.Entities
{
    public class User : BaseEntity
    {
        
        public User(Guid id,string userName, string email)
        {
            Id = id;
            UserName = userName;
            Email = email;
        }

        public string UserName { get; protected set; }
        public string Email { get; protected set; }

    }
}
