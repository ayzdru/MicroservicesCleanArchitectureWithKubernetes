using CleanArchitecture.Services.Payment.Core.Entities;
using CleanArchitecture.Services.Payment.Core.ValueObjects;
using CleanArchitecture.Services.Payment.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace CleanArchitecture.Services.Payment.Infrastructure.Data.Configurations
{
    public class OrderConfiguration : BaseConfiguration<Order>
    {
        public override void Configure(EntityTypeBuilder<Order> builder)
        {
            base.Configure(builder);
            builder.OwnsOne(
     o => o.TotalAmount,
     sa =>
     {
         sa.Property(p => p.Amount).HasColumnName(nameof(Order.TotalAmount)).IsRequired(Constants.Order.TotalAmountRequired);
         sa.Property(p => p.Currency).HasColumnName(nameof(Order.TotalAmount) + nameof(Money.Currency)).HasMaxLength(Constants.Money.CurrencyMaximumLength).IsRequired(Constants.Order.TotalAmountRequired);

     });
            builder.Navigation(o => o.TotalAmount)
                  .IsRequired(Constants.Order.TotalAmountRequired);
        }
    }
}

