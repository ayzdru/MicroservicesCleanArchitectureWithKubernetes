﻿using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CleanArchitecture.Services.Basket.API.DataTransferObjects.V1;
using CleanArchitecture.Services.Catalog.API.Grpc.V1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Grpc.Net.ClientFactory;

namespace CleanArchitecture.Services.Basket.API.Grpc.V1
{
    [Authorize]
    public class BasketServiceV1 : Basket.BasketBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDistributedCache _cache;
        private readonly Product.ProductClient _productClient;
        public BasketServiceV1(IHttpContextAccessor httpContextAccessor, IDistributedCache cache, GrpcClientFactory grpcClientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
            _productClient = grpcClientFactory.CreateClient<Product.ProductClient>(nameof(Product.ProductClient));
        }

        public override async Task<BasketsResponse> GetBasketItems(Empty request, ServerCallContext context)
        {
            var userId = _httpContextAccessor.HttpContext.User?.FindFirst(x => x.Type.Equals("sub"))?.Value;
           
            var basketResponse = new BasketsResponse();
            if (userId != null)
            {
                List<BasketItemDTO> basketItems = null;                
                var basketItemsJson = await _cache.GetStringAsync(userId);
                if (!string.IsNullOrEmpty(basketItemsJson))
                {
                    basketItems = JsonSerializer.Deserialize<List<BasketItemDTO>>(basketItemsJson);
                    foreach (var basketItem in basketItems)
                    {
                      var product =  await _productClient.GetProductByIdAsync(new GetProductByIdRequest()
                            {ProductId = basketItem.ProductId});

                      basketResponse.BasketItems.Add(new BasketItemResponse() { Name = product.Name, Description = product.Description, Price = product.Price, Quantity = basketItem.Quantity, ProductId = product.Id});
                    }
                }
            }
            return await Task.FromResult(basketResponse);
        }

        public override async Task<BoolValue> AddProductToBasket(BasketRequest request, ServerCallContext context)
        {
            BoolValue status = new BoolValue();
            var options = new DistributedCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromHours(2));
            var userId = _httpContextAccessor.HttpContext.User?.FindFirst(x => x.Type.Equals("sub"))?.Value;
            if (userId != null)
            {
                List<BasketItemDTO> basketItems;
                var basketItemsJson =  await _cache.GetStringAsync(userId);
                if (!string.IsNullOrEmpty(basketItemsJson))
                {
                    basketItems = JsonSerializer.Deserialize<List<BasketItemDTO>>(basketItemsJson);
                }
                else
                {
                    basketItems = new List<BasketItemDTO>();
                }

                var state = false;
                var basketItem = basketItems.Where(q => q.ProductId == request.ProductId).SingleOrDefault();
                if (basketItem == null)
                {
                    state = true;
                    basketItems.Add(new BasketItemDTO() { ProductId = request.ProductId, Quantity = 1 });
                }
                else
                {
                    basketItem.Quantity++;
                }
                var basketItemsSeriliazeJson = JsonSerializer.Serialize(basketItems);
                await _cache.SetStringAsync(userId, basketItemsSeriliazeJson, options);
                status.Value = state;
                return await Task.FromResult(status);
            }
            else
            {
                status.Value = false;
                return await Task.FromResult(status);
            }
        }

        public override async Task<Int32Value> GetBasketItemsCount(Empty request, ServerCallContext context)
        {
            Int32Value count = new Int32Value();
            List<BasketItemDTO> basketItems = null;
            var userId = _httpContextAccessor.HttpContext.User?.FindFirst(x => x.Type.Equals("sub"))?.Value;
            if (userId != null)
            {
                var basketItemsJson = await _cache.GetStringAsync(userId);
                if (!string.IsNullOrEmpty(basketItemsJson))
                {
                    basketItems = JsonSerializer.Deserialize<List<BasketItemDTO>>(basketItemsJson);
                    count.Value = basketItems.Count();
                }
            }
            return await Task.FromResult(count);
        }

        public override async Task<BoolValue> DeleteBasketItemByProductId(BasketRequest request, ServerCallContext context)
        {
            BoolValue status = new BoolValue();
            var options = new DistributedCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromHours(2));
            var userId = _httpContextAccessor.HttpContext.User?.FindFirst(x => x.Type.Equals("sub"))?.Value;
            if (userId != null)
            {
                List<BasketItemDTO> basketItems;
                var basketItemsJson = await _cache.GetStringAsync(userId);
                if (!string.IsNullOrEmpty(basketItemsJson))
                {
                    basketItems = JsonSerializer.Deserialize<List<BasketItemDTO>>(basketItemsJson);
                   var basketItem = basketItems.Where(b => b.ProductId == request.ProductId).SingleOrDefault();
                   if (basketItem != null)
                   {
                       basketItems.Remove(basketItem);
                       var basketItemsSeriliazeJson = JsonSerializer.Serialize(basketItems);
                       await _cache.SetStringAsync(userId, basketItemsSeriliazeJson, options);
                       status.Value = true;
                       return await Task.FromResult(status);
                    }
                }
            }
            status.Value = false;
            return await Task.FromResult(status);
        }

        public override async Task<BoolValue> ClearBasket(Empty request, ServerCallContext context)
        {
            BoolValue status = new BoolValue();
            var userId = _httpContextAccessor.HttpContext.User?.FindFirst(x => x.Type.Equals("sub"))?.Value;
            if (userId != null)
            {
                var basketItemsJson = await _cache.GetStringAsync(userId);
                if (!string.IsNullOrEmpty(basketItemsJson))
                {
                    await _cache.RemoveAsync(userId);
                    status.Value = true;
                    return await Task.FromResult(status);
                }
            }
            status.Value = false;
            return await Task.FromResult(status);
        }

        public override async Task<BoolValue> UpdateQuantityByProductId(UpdateQuantityRequest request, ServerCallContext context)
        {
            BoolValue status = new BoolValue();
            var options = new DistributedCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromHours(2));
            var userId = _httpContextAccessor.HttpContext.User?.FindFirst(x => x.Type.Equals("sub"))?.Value;
            if (userId != null)
            {
                List<BasketItemDTO> basketItems;
                var basketItemsJson = await _cache.GetStringAsync(userId);
                if (!string.IsNullOrEmpty(basketItemsJson))
                {
                    basketItems = JsonSerializer.Deserialize<List<BasketItemDTO>>(basketItemsJson);
                    var basketItem = basketItems.Where(b => b.ProductId == request.ProductId).SingleOrDefault();
                    if (basketItem != null)
                    {
                        basketItem.Quantity = request.Quantity;
                         var basketItemsSeriliazeJson = JsonSerializer.Serialize(basketItems);
                        await _cache.SetStringAsync(userId, basketItemsSeriliazeJson, options);
                        status.Value = true;
                        return await Task.FromResult(status);
                    }
                }
            }
            status.Value = false;
            return await Task.FromResult(status);
        }
    }
}
