using CleanArchitecture.Services.Order.Core.Interfaces;
using Confluent.Kafka;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Consul;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json.Linq;
using static Google.Rpc.Context.AttributeContext.Types;
using System.Linq;

namespace CleanArchitecture.Services.Order.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    public CurrentUserService(
        IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
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
        var token = await _httpContextAccessor.HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, "access_token");
        var httpClient = _httpClientFactory.CreateClient("IdentityServer");
        var identityUrl = _configuration.GetValue<string>("IdentityUrl");
        var disco = await httpClient.GetDiscoveryDocumentAsync(identityUrl);
        var response = await httpClient.GetUserInfoAsync(new UserInfoRequest
        {
            Address = disco.UserInfoEndpoint,
            Token = token
        });

        var name = response.Claims.FirstOrDefault(q=> q.Type ==  JwtRegisteredClaimNames.Name)?.Value;        
        return name;
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
