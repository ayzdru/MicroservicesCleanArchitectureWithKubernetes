using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Order.API
{
    public interface ITokenProvider
    {
        Task<string> GetTokenAsync();
    }

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
