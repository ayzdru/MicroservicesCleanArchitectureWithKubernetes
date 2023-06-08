using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Services.Catalog.Core.Entities;
using System.Threading;
using CleanArchitecture.Services.Catalog.Application.Notifications;
using CleanArchitecture.Services.Catalog.Application;
using CleanArchitecture.Services.Catalog.Application.Interfaces;
using CleanArchitecture.Services.Catalog.Core.Models;

namespace CleanArchitecture.Services.Catalog.Application.Commands
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
            private readonly ICatalogDbContext _applicationDbContext;
            private readonly IMediator _mediator;
            public CreateSubscriberUserCommandHandler(ICatalogDbContext applicationDbContext, IMediator mediator)
            {
                _applicationDbContext = applicationDbContext;
                _mediator = mediator;
            }
            public async Task<int> Handle(CreateSubscriberUserCommand request, CancellationToken cancellationToken)
            {
                _applicationDbContext.Users.Add(new User(request.SubscriberUser.Id, request.SubscriberUser.UserName, request.SubscriberUser.Email));                    
                var affected = await _applicationDbContext.SaveChangesAsync(cancellationToken);
                await _mediator.Publish(new CreateSubscriberUserNotification { SubscriberUser = request.SubscriberUser }, cancellationToken);
                return affected;
            }
        }
    }
}
