using CleanArchitecture.Services.Order.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Order.Core.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public Money Price { get; set; }
        public Product(Guid id, string name, string description, Money price)
        {
            Id = id;
            Name = name;
            Description = description;
            Price = price;
        }       
    }
}
