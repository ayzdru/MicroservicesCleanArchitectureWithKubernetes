using CleanArchitecture.Services.Payment.Application.Commands;
using CleanArchitecture.Services.Payment.Core.Interfaces;
using CleanArchitecture.Services.Payment.Core.Models;
using CleanArchitecture.Shared.DataProtection.Redis;
using DotNetCore.CAP;
using MediatR;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Payment.API.Services
{
    public class SubscriberService : ISubscriberService, ICapSubscribe
    {
        private readonly IMediator _mediator;

        public SubscriberService(IMediator mediator)
        {
            _mediator = mediator;
        }
        [CapSubscribe("OrderAdded")]
        public async Task OrderAdded(SubscriberOrderModel subscriberOrderModel)
        {
            await _mediator.Send(new CreateSubscriberOrderCommand(subscriberOrderModel));
        }

        [CapSubscribe("UserAdded")]
        public async Task UserAdded(SubscriberUserModel subscriberUserModel)
        {
            await _mediator.Send(new CreateSubscriberUserCommand(subscriberUserModel));
        }
        [CapSubscribe("UserDeleted")]
        public async Task UserDeleted(SubscriberUserModel subscriberUserModel)
        {
            await _mediator.Send(new DeleteSubscriberUserCommand(Guid.Parse(subscriberUserModel.Id)));
        }
        [CapSubscribe("UserUpdated")]
        public async Task UserUpdated(SubscriberUserModel subscriberUserModel)
        {
            await _mediator.Send(new UpdateSubscriberUserCommand(subscriberUserModel));
        }
    }
}
