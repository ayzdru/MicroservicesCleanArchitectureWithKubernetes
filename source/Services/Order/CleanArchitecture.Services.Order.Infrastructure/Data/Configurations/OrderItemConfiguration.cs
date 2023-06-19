using CleanArchitecture.Services.Order.Core.Entities;
using CleanArchitecture.Services.Order.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArchitecture.Services.Order.Infrastructure.Data.Configurations
{
    public class OrderItemConfiguration : BaseConfiguration<Core.Entities.OrderItem>
    {
        public override void Configure(EntityTypeBuilder<Core.Entities.OrderItem> builder)
        {
            base.Configure(builder);
            builder.Property(b => b.Quantity).IsRequired(Constants.OrderItem.QuantityRequired);
        }
    }
}
