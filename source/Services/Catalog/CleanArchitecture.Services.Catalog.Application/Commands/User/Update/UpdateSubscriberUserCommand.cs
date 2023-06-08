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
using Microsoft.EntityFrameworkCore;
using CleanArchitecture.Services.Catalog.Core.Interfaces;

namespace CleanArchitecture.Services.Catalog.Application.Commands
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
            private readonly ICatalogDbContext _catalogDbContext;
            private readonly ICurrentUserService _currentUserService;

            public UpdateSubscriberUserCommandHandler(ICatalogDbContext catalogDbContext, ICurrentUserService currentUserService)
            {
                _catalogDbContext= catalogDbContext;
                _currentUserService = currentUserService;
            }

            public async Task<int> Handle(UpdateSubscriberUserCommand request, CancellationToken cancellationToken)
            {                
                return await _catalogDbContext.Users.ExecuteUpdateAsync(q => q.SetProperty(a => a.UserName, request.SubscriberUser.UserName).SetProperty(a => a.Email, request.SubscriberUser.Email).SetProperty(b => b.LastModifiedByUserId, _currentUserService.UserId).SetProperty(b => b.LastModified, DateTime.Now), cancellationToken); ;
            }
        }
    }
}
