using CleanArchitecture.Services.Order.Application.Commands;
using CleanArchitecture.Services.Order.Core.Interfaces;
using CleanArchitecture.Services.Order.Core.Models;
using CleanArchitecture.Shared.DataProtection.Redis;
using DotNetCore.CAP;
using MediatR;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Order.API.Services
{
    public class SubscriberService : ISubscriberService, ICapSubscribe
    {
        private readonly IMediator _mediator;

        public SubscriberService(IMediator mediator)
        {
            _mediator = mediator;
        }

        [CapSubscribe("UserAdded")]
        public async Task UserAdded(SubscriberUserModel subscriberUserModel)
        {
            await _mediator.Send(new CreateSubscriberUserCommand(subscriberUserModel));
        }
        [CapSubscribe("UserDeleted")]
        public async Task UserDeleted(SubscriberUserModel subscriberUserModel)
        {
            await _mediator.Send(new DeleteSubscriberUserCommand(subscriberUserModel.Id));
        }
        [CapSubscribe("UserUpdated")]
        public async Task UserUpdated(SubscriberUserModel subscriberUserModel)
        {
            await _mediator.Send(new UpdateSubscriberUserCommand(subscriberUserModel));
        }
    }
}
