using CleanArchitecture.Services.Payment.Core.Entities;
using CleanArchitecture.Services.Payment.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArchitecture.Services.Payment.Infrastructure.Data.Configurations
{
    public class UserConfiguration : BaseConfiguration<User>
    {
        public override void Configure(EntityTypeBuilder<User> builder)
        {
            base.Configure(builder);
            builder.Property(b => b.UserName).HasMaxLength(Constants.User.UserNameMaximumLength).IsRequired(Constants.User.UserNameRequired);
        }        
    }
}
