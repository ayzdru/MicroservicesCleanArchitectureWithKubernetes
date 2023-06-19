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
using CleanArchitecture.Services.Catalog.Application.Extensions;

namespace CleanArchitecture.Services.Catalog.Application.Queries
{
    public class GetProductByIdQuery :  IRequest<ProductModel>
    {
        public Guid ProductId { get; set; }

        public GetProductByIdQuery(Guid productId)
        {
            ProductId = productId;
        }

        public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductModel>
        {
            private readonly ICatalogDbContext _catalogDbContext;

            public GetProductByIdQueryHandler(ICatalogDbContext catalogDbContext)
            {
                _catalogDbContext = catalogDbContext;
            }

            public async Task<ProductModel> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
            {
                return await _catalogDbContext.Products.GetById(request.ProductId).Select(q => new ProductModel(q.Id, q.Name, q.Description, q.Price.Amount, q.Price.Currency.Name)).SingleOrDefaultAsync(cancellationToken);
            }
        }
    }
}
