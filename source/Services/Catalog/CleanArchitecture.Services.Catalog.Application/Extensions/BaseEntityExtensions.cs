using CleanArchitecture.Services.Catalog.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CleanArchitecture.Services.Catalog.Application.Extensions
{
    public static class BaseEntityExtensions
    {
        public static IQueryable<T> GetById<T>(this DbSet<T> dbSet, Guid id) where T : BaseEntity
        {
            return dbSet.Where(q => q.Id == id);
        }
        public static IQueryable<T> GetAll<T>(this DbSet<T> dbSet) where T : BaseEntity
        {
            return dbSet;
        }
    }
}
