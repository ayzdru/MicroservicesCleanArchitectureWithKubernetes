﻿using CleanArchitecture.Services.Basket.Core.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Basket.API
{

    public class AppTokenProvider : ITokenProvider
    {
        private string _token;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppTokenProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> GetTokenAsync()
        {
            if (_token == null)
            {
                _token = await _httpContextAccessor.HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, "access_token");
            }

            return _token;
        }
    }
}
