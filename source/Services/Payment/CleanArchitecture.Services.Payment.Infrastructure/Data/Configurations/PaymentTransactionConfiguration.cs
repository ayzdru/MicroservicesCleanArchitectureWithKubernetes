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
    public class PaymentTransactionConfiguration : BaseConfiguration<PaymentTransaction>
    {
        public override void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            base.Configure(builder);
            builder.OwnsOne(
    o => o.Amount,
    sa =>
    {
        sa.Property(p => p.Amount).HasColumnName(nameof(PaymentTransaction.Amount)).IsRequired(Constants.PaymentTransaction.AmountRequired);
        sa.Property(p => p.Currency).HasColumnName(nameof(PaymentTransaction.Amount) + nameof(Money.Currency)).HasMaxLength(Constants.Money.CurrencyMaximumLength).IsRequired(Constants.PaymentTransaction.AmountRequired);

    });
            builder.Navigation(o => o.Amount)
                 .IsRequired(Constants.PaymentTransaction.AmountRequired);
        }
    }
}
