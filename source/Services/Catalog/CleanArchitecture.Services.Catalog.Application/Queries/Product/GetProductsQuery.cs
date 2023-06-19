using CleanArchitecture.Services.Catalog.Application.Interfaces;
using CleanArchitecture.Services.Catalog.Application.Models;
using CleanArchitecture.Services.Catalog.Core.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Catalog.Application.Queries
{
    public class GetProductsQuery :  IRequest<List<ProductModel>>
    {      
        public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<ProductModel>>
        {
            private readonly ICatalogDbContext _catalogDbContext;

            public GetProductsQueryHandler(ICatalogDbContext catalogDbContext)
            {
                _catalogDbContext = catalogDbContext;
            }

            public Task<List<ProductModel>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
            {
                return _catalogDbContext.Products.Select(q => new ProductModel(q.Id,q.Name,q.Description,q.Price.Amount, q.Price.Currency)).ToListAsync(cancellationToken);
            }
        }
    }
}
