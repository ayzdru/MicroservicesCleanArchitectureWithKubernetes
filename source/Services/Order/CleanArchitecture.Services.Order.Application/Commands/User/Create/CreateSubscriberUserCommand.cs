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

namespace CleanArchitecture.Services.Order.Application.Commands
{
    public class CreateSubscriberUserCommand : IRequest<int>
    {
        public SubscriberUserModel SubscriberUser { get; set; }
        public CreateSubscriberUserCommand(SubscriberUserModel subscriberUser)
        {

            SubscriberUser = subscriberUser;
        }
        public class CreateSubscriberUserCommandHandler : IRequestHandler<CreateSubscriberUserCommand, int>
        {
            private readonly IOrderDbContext _orderDbContext;
            private readonly IMediator _mediator;
            public CreateSubscriberUserCommandHandler(IOrderDbContext orderDbContext, IMediator mediator)
            {
                _orderDbContext = orderDbContext;
                _mediator = mediator;
            }
            public async Task<int> Handle(CreateSubscriberUserCommand request, CancellationToken cancellationToken)
            {
                _orderDbContext.Users.Add(new User(request.SubscriberUser.Id, request.SubscriberUser.UserName, request.SubscriberUser.Email));                    
                var affected = await _orderDbContext.SaveChangesAsync(cancellationToken);
                await _mediator.Publish(new CreateSubscriberUserNotification { SubscriberUser = request.SubscriberUser }, cancellationToken);
                return affected;
            }
        }
    }
}
