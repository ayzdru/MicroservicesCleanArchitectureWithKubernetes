using CleanArchitecture.Services.Catalog.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Catalog.Core.Entities
{
    public class Product : BaseEntity
    {      
        public string Name { get; private set; }
        public string Description { get; private set; }
        public Money Price { get; private set; }
        public Product(string name, string description, Money price)
        {
            Name = name;
            Description = description;
            Price = price;
        }
    }
}
