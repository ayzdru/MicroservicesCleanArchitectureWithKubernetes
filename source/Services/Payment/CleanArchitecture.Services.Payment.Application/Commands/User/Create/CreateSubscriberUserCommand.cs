using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Services.Payment.Core.Entities;
using System.Threading;
using CleanArchitecture.Services.Payment.Application.Notifications;
using CleanArchitecture.Services.Payment.Application;
using CleanArchitecture.Services.Payment.Application.Interfaces;
using CleanArchitecture.Services.Payment.Core.Models;

namespace CleanArchitecture.Services.Payment.Application.Commands
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
            private readonly IPaymentDbContext _paymentDbContext;
            private readonly IMediator _mediator;
            public CreateSubscriberUserCommandHandler(IPaymentDbContext paymentDbContext, IMediator mediator)
            {
                _paymentDbContext = paymentDbContext;
                _mediator = mediator;
            }
            public async Task<int> Handle(CreateSubscriberUserCommand request, CancellationToken cancellationToken)
            {
                _paymentDbContext.Users.Add(new User(request.SubscriberUser.Id, request.SubscriberUser.UserName, request.SubscriberUser.Email));                    
                var affected = await _paymentDbContext.SaveChangesAsync(cancellationToken);
                await _mediator.Publish(new CreateSubscriberUserNotification { SubscriberUser = request.SubscriberUser }, cancellationToken);
                return affected;
            }
        }
    }
}
