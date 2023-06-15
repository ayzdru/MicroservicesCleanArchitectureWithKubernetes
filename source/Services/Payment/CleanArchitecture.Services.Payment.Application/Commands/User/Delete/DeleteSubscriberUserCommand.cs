using CleanArchitecture.Services.Payment.Application.Extensions;
using CleanArchitecture.Services.Payment.Application.Interfaces;
using CleanArchitecture.Services.Payment.Core.Entities;
using CleanArchitecture.Services.Payment.Core.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Payment.Application.Commands
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
            private readonly IPaymentDbContext  _paymentDbContext;

            public DeleteSubscriberUserCommandHandler(IPaymentDbContext paymentDbContext)
            {
                _paymentDbContext = paymentDbContext;
            }

            public async Task<int> Handle(DeleteSubscriberUserCommand request, CancellationToken cancellationToken)
            {
                return await _paymentDbContext.Users.GetById(request.Id).ExecuteDeleteAsync(cancellationToken);
            }
        }
    }
}
