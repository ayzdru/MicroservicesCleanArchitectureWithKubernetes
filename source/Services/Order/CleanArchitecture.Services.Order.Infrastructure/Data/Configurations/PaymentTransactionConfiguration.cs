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
    public class PaymentTransactionConfiguration : BaseConfiguration<PaymentTransaction>
    {
        public override void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            base.Configure(builder);
            builder.OwnsOne(
    o => o.Amount,
    sa =>
    {
        sa.Property(p => p.Amount).HasColumnName(nameof(Money.Amount)).IsRequired(Constants.Money.AmountRequired);
        sa.OwnsOne(c => c.Currency, ca =>
        {
            ca.Property(p=> p.Symbol).HasColumnName(nameof(Currency.Symbol)).HasMaxLength(Constants.Currency.SymbolMaximumLength).IsRequired(Constants.Currency.SymbolRequired);
            ca.Property(p=> p.Name).HasColumnName(nameof(Currency.Name)).HasMaxLength(Constants.Currency.NameMaximumLength).IsRequired(Constants.Currency.NameRequired);
        });
    });
        }
    }
}
