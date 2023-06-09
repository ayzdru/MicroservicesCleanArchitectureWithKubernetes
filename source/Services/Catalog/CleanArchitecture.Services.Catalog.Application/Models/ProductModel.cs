﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Catalog.Application.Models
{
    public record ProductModel
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public string Description { get; init; }
        public decimal Price { get; init; }
        public string PriceCurrency { get; init; }
        public ProductModel(Guid id, string name, string description, decimal price, string priceCurrency)
        {
            Id = id;
            Name = name;
            Description = description;
            Price = price;
            PriceCurrency = priceCurrency;
        }

    }
}
