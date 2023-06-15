using System;

namespace CleanArchitecture.Services.Payment.Core.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
}
