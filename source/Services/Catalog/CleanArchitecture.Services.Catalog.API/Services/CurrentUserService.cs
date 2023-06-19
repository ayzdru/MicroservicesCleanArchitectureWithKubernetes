using CleanArchitecture.Services.Catalog.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Catalog.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public CurrentUserService(
        IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }
    public Guid? UserId
    {
        get
        {

            string userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (!string.IsNullOrEmpty(userId))
            {
                if (Guid.TryParse(userId, out Guid _userId))
                {
                    return _userId;
                }
            }
            return null;
        }
    }
  

    public async Task<string?> GetUserNameAsync(Guid userId)
    {
        var nameIdentifier = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (nameIdentifier == userId.ToString())
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
