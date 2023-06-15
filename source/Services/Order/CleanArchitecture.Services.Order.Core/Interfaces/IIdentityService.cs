using CleanArchitecture.Services.Order.Core.Models;
using System;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Order.Core.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(Guid userId);

    Task<bool> IsInRoleAsync(Guid userId, string role);

    Task<bool> AuthorizeAsync(Guid userId, string policyName);
}
