using CleanArchitecture.Services.Order.Core.Entities;
using CleanArchitecture.Services.Order.Core.ValueObjects;
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
            builder.OwnsOne(
   o => o.TotalAmount,
   sa =>
   {
       sa.Property(p => p.Amount).HasColumnName(nameof(OrderItem.TotalAmount)).IsRequired(Constants.Order.TotalAmountRequired);
       sa.Property(p => p.Currency).HasColumnName(nameof(OrderItem.TotalAmount) + nameof(Money.Currency)).HasMaxLength(Constants.Money.CurrencyMaximumLength).IsRequired(Constants.Order.TotalAmountRequired);

   });
            builder.Navigation(o => o.TotalAmount)
                  .IsRequired(Constants.Order.TotalAmountRequired);
        }
    }
}
