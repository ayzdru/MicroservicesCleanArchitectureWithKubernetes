using CleanArchitecture.Services.Payment.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;


namespace CleanArchitecture.Services.Payment.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    public Guid? UserId
    {
        get
        {

            string userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
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
}
