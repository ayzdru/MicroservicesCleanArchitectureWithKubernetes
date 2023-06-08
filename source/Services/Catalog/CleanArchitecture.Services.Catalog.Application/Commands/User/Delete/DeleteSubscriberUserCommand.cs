using CleanArchitecture.Services.Catalog.Application.Extensions;
using CleanArchitecture.Services.Catalog.Application.Interfaces;
using CleanArchitecture.Services.Catalog.Core.Entities;
using CleanArchitecture.Services.Catalog.Core.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Catalog.Application.Commands
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
            private readonly ICatalogDbContext  _catalogDbContext;

            public DeleteSubscriberUserCommandHandler(ICatalogDbContext catalogDbContext)
            {
                _catalogDbContext = catalogDbContext;
            }

            public async Task<int> Handle(DeleteSubscriberUserCommand request, CancellationToken cancellationToken)
            {
                return await _catalogDbContext.Users.GetById(request.Id).ExecuteDeleteAsync(cancellationToken);
            }
        }
    }
}
