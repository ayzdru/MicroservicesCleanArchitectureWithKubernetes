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
      sa.Property(p => p.Amount).HasColumnName(nameof(Product.Price)).IsRequired(Constants.Product.PriceRequired);
      sa.Property(p => p.Currency).HasColumnName(nameof(Product.Price) + nameof(Money.Currency)).HasMaxLength(Constants.Money.CurrencyMaximumLength).IsRequired(Constants.Product.PriceRequired);
  });
            builder.Navigation(o => o.Price)
                  .IsRequired(Constants.Product.PriceRequired);
        }
    }
}
