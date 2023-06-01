using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Services.Order.API.Data;
using CleanArchitecture.Services.Order.API.Entities;
using DotNetCore.CAP;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.ClientFactory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using static CleanArchitecture.Services.Basket.API.Grpc.V1.Basket;

namespace CleanArchitecture.Services.Order.API.Grpc.V1
{
    [Authorize]
    public class OrderServiceV1 : Order.OrderBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly BasketClient _basketClient;
        private readonly OrderDbContext _orderDbContext;
        private readonly ICapPublisher _capBus;

        public OrderServiceV1(IHttpContextAccessor httpContextAccessor, GrpcClientFactory grpcClientFactory, OrderDbContext orderDbContext, ICapPublisher capBus)
        {
            _httpContextAccessor = httpContextAccessor;
            _basketClient = grpcClientFactory.CreateClient<BasketClient>(nameof(BasketClient));
            _orderDbContext = orderDbContext;
            _capBus = capBus;
        }

        public override async Task<BoolValue> Checkout(Empty request, ServerCallContext context)
        {
            BoolValue status = new BoolValue();
            var userIdValue = _httpContextAccessor.HttpContext.User?.FindFirst(x => x.Type.Equals("sub"))?.Value;
            if (userIdValue != null && Guid.TryParse(userIdValue, out Guid userId))
            {
                using (var transaction = _orderDbContext.Database.BeginTransaction(_capBus, autoCommit: false))
                {
                    var user = _orderDbContext.Users.Where(q => q.Id == userId).SingleOrDefault();
                    if (user == null)
                    {
                        _orderDbContext.Users.Add(new User() { Id = userId });
                    }
                    var getBasketItemsResponse = await _basketClient.GetBasketItemsAsync(new Empty());
                    if (getBasketItemsResponse.BasketItems.Count > 0)
                    {

                        var newOrder = _orderDbContext.Orders.Add(new Entities.Order(Guid.NewGuid(), getBasketItemsResponse.BasketItems.Sum(q => q.Price * q.Quantity)));
                        foreach (var basketItem in getBasketItemsResponse.BasketItems)
                        {
                            if (Guid.TryParse(basketItem.ProductId, out Guid productId))
                            {
                                var product = _orderDbContext.Products.Where(q => q.Id == productId).SingleOrDefault();
                                if (product == null)
                                {
                                    _orderDbContext.Products.Add(new Product(productId, basketItem.Name, basketItem.Description, basketItem.Price));
                                }
                                newOrder.Entity.AddOrderItem(new OrderItem() { ProductId = productId, TotalAmount = basketItem.Price * basketItem.Quantity });
                            }
                            else
                            {
                                await transaction.RollbackAsync();
                                status.Value = false;
                                return await Task.FromResult(status);
                            }
                        }
                        await _capBus.PublishAsync("AddPayment", newOrder.Entity.Id);
                        await _orderDbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                        await _basketClient.ClearBasketAsync(new Empty());
                        status.Value = true;
                        return await Task.FromResult(status);
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        status.Value = false;
                        return await Task.FromResult(status);
                    }
                }
            }
            status.Value = false;
            return await Task.FromResult(status);
        }

    }
}
