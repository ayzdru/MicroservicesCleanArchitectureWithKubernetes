using CleanArchitecture.Services.Catalog.Core.Entities;
using CleanArchitecture.Services.Catalog.Core.ValueObjects;
using CleanArchitecture.Services.Catalog.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArchitecture.Services.Catalog.Infrastructure.Data.Configurations
{
    public class ProductConfiguration : BaseConfiguration<Product>
    {
        public override void Configure(EntityTypeBuilder<Product> builder)
        {
            base.Configure(builder);
            builder.Property(b => b.Name).HasMaxLength(Constants.Product.NameMaximumLength).IsRequired(Constants.Product.NameRequired);
            builder.Property(b => b.Description).HasMaxLength(Constants.Product.DescriptionMaximumLength).IsRequired(Constants.Product.DescriptionRequired);
            builder.OwnsOne(
  o => o.Price,
  sa =>
  {
      sa.Property(p => p.Amount).HasColumnName(nameof(Money.Amount)).IsRequired(Constants.Product.PriceRequired);
      sa.OwnsOne(c => c.Currency, ca =>
      {
          ca.Property(p => p.Symbol).HasColumnName(nameof(Currency.Symbol)).HasMaxLength(Constants.Currency.SymbolMaximumLength).IsRequired(Constants.Product.PriceRequired);
          ca.Property(p => p.Name).HasColumnName(nameof(Currency.Name)).HasMaxLength(Constants.Currency.NameMaximumLength).IsRequired(Constants.Product.PriceRequired);
      });
      sa.Navigation(q => q.Currency).IsRequired(Constants.Product.PriceRequired);
  });
            builder.Navigation(o => o.Price)
                  .IsRequired(Constants.Product.PriceRequired);
        }
    }
}
