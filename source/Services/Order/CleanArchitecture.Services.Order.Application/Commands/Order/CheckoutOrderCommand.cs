using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Services.Order.Core.Entities;
using System.Threading;
using CleanArchitecture.Services.Order.Application.Notifications;
using CleanArchitecture.Services.Order.Application;
using CleanArchitecture.Services.Order.Application.Interfaces;
using CleanArchitecture.Services.Order.Core.Models;
using CleanArchitecture.Services.Order.Core.ValueObjects;
using CleanArchitecture.Services.Order.Core.Interfaces;
using DotNetCore.CAP;
using CleanArchitecture.Services.Order.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Services.Order.Application.Commands
{
    public class CheckoutOrderCommand : IRequest<bool>
    {
        private readonly List<BasketItemModel> BasketItems;

        public CheckoutOrderCommand(List<BasketItemModel> basketItems)
        {
            BasketItems = basketItems;
        }

        public class CreateSubscriberOrderCommandHandler : IRequestHandler<CheckoutOrderCommand, bool>
        {
            private readonly IOrderDbContext _orderDbContext;
            private readonly ICapPublisher _capBus;
            private readonly ICurrentUserService _currentUserService;
           
            public CreateSubscriberOrderCommandHandler(IOrderDbContext orderDbContext, ICapPublisher capBus, ICurrentUserService currentUserService)
            {
                _orderDbContext = orderDbContext;
                _capBus = capBus;
                _currentUserService = currentUserService;
            }
            public async Task<bool> Handle(CheckoutOrderCommand request, CancellationToken cancellationToken)
            {

                if (_currentUserService.UserId != null && request.BasketItems != null && request.BasketItems.Count > 0)
                {
                    using (var transaction = _orderDbContext.Database.BeginTransaction(_capBus, autoCommit: false))
                    {                       
                        var user = _orderDbContext.Users.Where(q => q.Id == _currentUserService.UserId.Value).SingleOrDefault();
                        if (user == null)
                        {
                            var username = await _currentUserService.GetUserNameAsync(_currentUserService.UserId.Value);
                            _orderDbContext.Users.Add(new User(_currentUserService.UserId.Value, username, username));
                        }
                        var newOrder = _orderDbContext.Orders.Add(new Core.Entities.Order(Guid.NewGuid(), new Money(request.BasketItems.Sum(q => q.Price * q.Quantity), Currency.USD)));
                        foreach (var basketItem in request.BasketItems)
                        {
                            var product = _orderDbContext.Products.Where(q => q.Id == basketItem.ProductId).SingleOrDefault();
                            if (product == null)
                            {
                                _orderDbContext.Products.Add(new Product(basketItem.ProductId, basketItem.Name, basketItem.Description, new Money(basketItem.Price, Currency.USD)));
                            }
                            newOrder.Entity.AddOrderItem(new OrderItem(Guid.NewGuid(), basketItem.ProductId, basketItem.Quantity, new Money(basketItem.Price * basketItem.Quantity, Currency.USD)));
                        }

                        await _capBus.PublishAsync("OrderAdded", newOrder.Entity.Id);
                        await _orderDbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                        return true;
                        
                    }
                }
                return false;
            }
        }
    }
}
