using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace CleanArchitecture.Services.Order.Core.Extensions
{
    public static class UserExtensions
    {
        public static Guid? GetUserId(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                return (Guid?)null;
            }
            return Guid.TryParse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId) ? userId : (Guid?)null;
        }
    }
}
