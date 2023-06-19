using CleanArchitecture.Services.Payment.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArchitecture.Services.Payment.Infrastructure.Common
{
    public abstract class BaseConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id).ValueGeneratedOnAdd();
            builder.Property(p => p.RowVersion).IsRowVersion(); 
            
            builder.HasOne(q => q.CreatedByUser).WithMany().HasForeignKey(p => p.CreatedByUserId).IsRequired(false);
            builder.HasOne(q => q.LastModifiedByUser).WithMany().HasForeignKey(p => p.LastModifiedByUserId).IsRequired(false);
        }
    }
}
