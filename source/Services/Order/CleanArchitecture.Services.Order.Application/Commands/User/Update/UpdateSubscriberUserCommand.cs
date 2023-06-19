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
using Microsoft.EntityFrameworkCore;
using CleanArchitecture.Services.Order.Core.Interfaces;

namespace CleanArchitecture.Services.Order.Application.Commands
{
    public class UpdateSubscriberUserCommand : IRequest<int>
    {
        public SubscriberUserModel SubscriberUser { get; set; }
        public UpdateSubscriberUserCommand(SubscriberUserModel subscriberUser)
        {

            SubscriberUser = subscriberUser;
        }
        public class UpdateSubscriberUserCommandHandler : IRequestHandler<UpdateSubscriberUserCommand, int>
        {
            private readonly IOrderDbContext _orderDbContext;
            private readonly ICurrentUserService _currentUserService;

            public UpdateSubscriberUserCommandHandler(IOrderDbContext orderDbContext, ICurrentUserService currentUserService)
            {
                _orderDbContext = orderDbContext;
                _currentUserService = currentUserService;
            }

            public async Task<int> Handle(UpdateSubscriberUserCommand request, CancellationToken cancellationToken)
            {                
                return await _orderDbContext.Users.ExecuteUpdateAsync(q => q.SetProperty(a => a.UserName, request.SubscriberUser.UserName).SetProperty(a => a.Email, request.SubscriberUser.Email).SetProperty(b => b.LastModifiedByUserId, _currentUserService.UserId).SetProperty(b => b.LastModified, DateTime.Now), cancellationToken); ;
            }
        }
    }
}
