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
using Microsoft.EntityFrameworkCore;
using CleanArchitecture.Services.Payment.Core.Interfaces;

namespace CleanArchitecture.Services.Payment.Application.Commands
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
            private readonly IPaymentDbContext _paymentDbContext;
            private readonly ICurrentUserService _currentUserService;

            public UpdateSubscriberUserCommandHandler(IPaymentDbContext paymentDbContext, ICurrentUserService currentUserService)
            {
                _paymentDbContext = paymentDbContext;
                _currentUserService = currentUserService;
            }

            public async Task<int> Handle(UpdateSubscriberUserCommand request, CancellationToken cancellationToken)
            {                
                return await _paymentDbContext.Users.ExecuteUpdateAsync(q => q.SetProperty(a => a.UserName, request.SubscriberUser.UserName).SetProperty(a => a.Email, request.SubscriberUser.Email).SetProperty(b => b.LastModifiedByUserId, _currentUserService.UserId).SetProperty(b => b.LastModified, DateTime.Now), cancellationToken); ;
            }
        }
    }
}
