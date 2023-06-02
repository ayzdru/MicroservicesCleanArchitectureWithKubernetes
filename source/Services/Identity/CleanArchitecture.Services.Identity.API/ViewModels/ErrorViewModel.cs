using Duende.IdentityServer.Models;

namespace CleanArchitecture.Services.Identity.API.ViewModels
{
    public class ErrorViewModel
    {
        public ErrorViewModel()
        {
                
        }
        public ErrorViewModel(string error)
        {
            Error = new ErrorMessage { Error = error };
        }

        public ErrorMessage Error { get; set; }
    }
}
