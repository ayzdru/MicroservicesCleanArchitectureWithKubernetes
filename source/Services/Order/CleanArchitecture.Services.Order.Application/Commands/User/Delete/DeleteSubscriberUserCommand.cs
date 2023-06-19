using CleanArchitecture.Services.Order.Application.Extensions;
using CleanArchitecture.Services.Order.Application.Interfaces;
using CleanArchitecture.Services.Order.Core.Entities;
using CleanArchitecture.Services.Order.Core.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Order.Application.Commands
{    
    public class DeleteSubscriberUserCommand : IRequest<int>
    {
        public DeleteSubscriberUserCommand(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }

        public class DeleteSubscriberUserCommandHandler : IRequestHandler<DeleteSubscriberUserCommand, int>
        {
            private readonly IOrderDbContext _orderDbContext;

            public DeleteSubscriberUserCommandHandler(IOrderDbContext orderDbContext)
            {
                _orderDbContext = orderDbContext;
            }

            public async Task<int> Handle(DeleteSubscriberUserCommand request, CancellationToken cancellationToken)
            {
                return await _orderDbContext.Users.GetById(request.Id).ExecuteDeleteAsync(cancellationToken);
            }
        }
    }
}
