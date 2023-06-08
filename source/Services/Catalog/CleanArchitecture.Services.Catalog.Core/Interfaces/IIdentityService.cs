using CleanArchitecture.Services.Catalog.Core.Models;
using System;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Catalog.Core.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(Guid userId);

    Task<bool> IsInRoleAsync(Guid userId, string role);

    Task<bool> AuthorizeAsync(Guid userId, string policyName);
}
