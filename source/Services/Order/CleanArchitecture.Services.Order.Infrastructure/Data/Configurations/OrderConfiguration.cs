using CleanArchitecture.Services.Order.Core.Entities;
using CleanArchitecture.Services.Order.Core.ValueObjects;
using CleanArchitecture.Services.Order.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace CleanArchitecture.Services.Order.Infrastructure.Data.Configurations
{
    public class OrderConfiguration : BaseConfiguration<Core.Entities.Order>
    {
        public override void Configure(EntityTypeBuilder<Core.Entities.Order> builder)
        {
            base.Configure(builder);
            builder.OwnsOne(
    o => o.TotalAmount,
    sa =>
    {
        sa.Property(p => p.Amount).HasColumnName(nameof(Money.Amount)).IsRequired(Constants.Order.TotalAmountRequired);
        sa.OwnsOne(c => c.Currency, ca =>
        {
            ca.Property(p => p.Symbol).HasColumnName(nameof(Currency.Symbol)).HasMaxLength(Constants.Currency.SymbolMaximumLength).IsRequired(Constants.Order.TotalAmountRequired);
            ca.Property(p => p.Name).HasColumnName(nameof(Currency.Name)).HasMaxLength(Constants.Currency.NameMaximumLength).IsRequired(Constants.Order.TotalAmountRequired);
        });
        sa.Navigation(q => q.Currency).IsRequired(Constants.Order.TotalAmountRequired);
    });
            builder.Navigation(o => o.TotalAmount)
                  .IsRequired(Constants.Order.TotalAmountRequired);
        }
    }
}
