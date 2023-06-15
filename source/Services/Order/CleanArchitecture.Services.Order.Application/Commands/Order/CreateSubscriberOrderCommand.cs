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

namespace CleanArchitecture.Services.Order.Application.Commands
{
    public class CreateSubscriberOrderCommand : IRequest<int>
    {
        public SubscriberOrderModel SubscriberOrder { get; set; }
        public CreateSubscriberOrderCommand(SubscriberOrderModel subscriberOrder)
        {

            SubscriberOrder = subscriberOrder;
        }
        public class CreateSubscriberOrderCommandHandler : IRequestHandler<CreateSubscriberOrderCommand, int>
        {
            private readonly IPaymentDbContext  _paymentDbContext;
            private readonly IMediator _mediator;
            public CreateSubscriberOrderCommandHandler(IPaymentDbContext paymentDbContext, IMediator mediator)
            {
                _paymentDbContext = paymentDbContext;
                _mediator = mediator;
            }
            public async Task<int> Handle(CreateSubscriberOrderCommand request, CancellationToken cancellationToken)
            {
                _paymentDbContext.Orders.Add(new Core.Entities.Order(request.SubscriberOrder.OrderId, new Money(request.SubscriberOrder.TotalAmount, new Currency(request.SubscriberOrder.CurrencyName, request.SubscriberOrder.CurrencySymbol))));                    
                var affected = await _paymentDbContext.SaveChangesAsync(cancellationToken);
                await _mediator.Publish(new CreateSubscriberOrderNotification { SubscriberOrder = request.SubscriberOrder }, cancellationToken);
                return affected;
            }
        }
    }
}
