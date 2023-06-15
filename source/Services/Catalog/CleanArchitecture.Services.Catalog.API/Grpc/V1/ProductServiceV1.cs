using System;
using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Services.Catalog.Application.Queries;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;

namespace CleanArchitecture.Services.Catalog.API.Grpc.V1
{   
    public class ProductServiceV1 : Product.ProductBase
    {
        private readonly IMediator _mediator;

        public ProductServiceV1(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override async Task<ProductsResponse> GetProducts(Empty request, ServerCallContext context)
        {
            var productsResponse = new ProductsResponse();
            var products = await _mediator.Send(new GetProductsQuery());
            productsResponse.Products.AddRange(products.Select(q=> new ProductResponse() { Id = q.Id.ToString(), Name = q.Name, Description = q.Description, Price = q.Price  }));
            return await Task.FromResult(productsResponse); 
        }

        public override async Task<ProductResponse> GetProductById(GetProductByIdRequest request, ServerCallContext context)
        {
            var product = await _mediator.Send(new GetProductByIdQuery(Guid.Parse(request.ProductId)));
            return await Task.FromResult( new ProductResponse() { Id = product.Id.ToString(), Name = product.Name, Description = product.Description, Price = product.Price });
        }
    }
}
