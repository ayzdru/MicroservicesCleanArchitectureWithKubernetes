using System;

namespace CleanArchitecture.Services.Order.Core.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
}
