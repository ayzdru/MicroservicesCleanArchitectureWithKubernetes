using System;

namespace CleanArchitecture.Services.Catalog.Core.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
}
