using System;

namespace CleanArchitecture.Services.Catalog.Core.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    Task<string?> GetUserNameAsync(Guid userId);

    Task<bool> IsInRoleAsync(Guid userId, string role);

    Task<bool> AuthorizeAsync(Guid userId, string policyName);
}
