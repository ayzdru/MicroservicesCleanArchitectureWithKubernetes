using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Services.Order.Application.Commands;
using CleanArchitecture.Services.Order.Application.Models;
using DotNetCore.CAP;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.ClientFactory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using static CleanArchitecture.Services.Basket.API.Grpc.V1.Basket;

namespace CleanArchitecture.Services.Order.API.Grpc.V1
{
    [Authorize]
    public class OrderServiceV1 : Order.OrderBase
    {
        private readonly BasketClient _basketClient;
        private readonly IMediator _mediator;
        public OrderServiceV1(BasketClient basketClient, IMediator mediator)
        {
            _basketClient = basketClient;
            _mediator = mediator;
        }

        public override async Task<BoolValue> Checkout(Empty request, ServerCallContext context)
        {
            BoolValue status = new BoolValue();
            var basketsResponse = await _basketClient.GetBasketItemsAsync(new Empty());
            if (basketsResponse.BasketItems.Count > 0)
            {
                status.Value = await _mediator.Send(new CheckoutOrderCommand(basketsResponse.BasketItems.Select(q=> new BasketItemModel(Guid.Parse(q.ProductId),q.Name, q.Description, q.Quantity, q.Price)).ToList()));
            }               
            if(status.Value == true)
            {
                await _basketClient.ClearBasketAsync(new Empty());
            }
            return await Task.FromResult(status);
        }

    }
}
