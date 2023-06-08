using CleanArchitecture.Services.Catalog.Core.Entities;
using CleanArchitecture.Services.Catalog.Core.Interfaces;
using CleanArchitecture.Services.Catalog.Core.Models;
using CleanArchitecture.Services.Catalog.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Catalog.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public IdentityService(
        IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string?> GetUserNameAsync(Guid userId)
    {
        var nameIdentifier = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(nameIdentifier == userId.ToString())
        {
            return _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
        }
        return null;
    }

  
    public async Task<bool> IsInRoleAsync(Guid userId, string role)
    {
        var nameIdentifier = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (nameIdentifier == userId.ToString())
        {
            var isInRole = _httpContextAccessor.HttpContext?.User.IsInRole(role);
            return isInRole.HasValue ? isInRole.Value : false;
        }
        return false;
    }

    public async Task<bool> AuthorizeAsync(Guid userId, string policyName)
    {
        if (_httpContextAccessor.HttpContext.User != null)
        {
            var result = await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, policyName);

            return result.Succeeded;
        }
        else
        {
            return false;
        }
    }
}
