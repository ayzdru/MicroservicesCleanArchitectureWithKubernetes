using CleanArchitecture.Services.Catalog.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Catalog.Core.Entities
{
    public class Product : BaseEntity
    {      
        public string Name { get;  set; }
        public string Description { get;  set; }
        public Money Price { get; set; }
        public Product(string name, string description, Money price)
        {
            Name = name;
            Description = description;
            Price = price;
        }

        public Product(string name, string description)
        {
            Name = name;
            Description = description;
        }
        public Product()
        {
                
        }
    }
}
