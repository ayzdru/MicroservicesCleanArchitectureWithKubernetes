using CleanArchitecture.Services.Payment.Core.Interfaces;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Payment.Application.Behaviours;

public class LoggingBehaviour<TRequest> : IRequestPreProcessor<TRequest> where TRequest : notnull
{
    private readonly ILogger _logger;
    private readonly ICurrentUserService _currentUserService;

    public LoggingBehaviour(ILogger<TRequest> logger, ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUserService.UserId ?? Guid.Empty;
        string? userName = string.Empty;

        if (_currentUserService.UserId.HasValue)
        {
            userName = await _currentUserService.GetUserNameAsync(userId);
        }

        _logger.LogInformation("CleanArchitecture Request: {Name} {@UserId} {@UserName} {@Request}",
            requestName, userId, userName, request);
    }
}
