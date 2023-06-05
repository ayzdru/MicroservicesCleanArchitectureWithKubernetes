using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace CleanArchitecture.Web.BlazorWebAssembly.Client
{
    public class StreamingHttpHandler : DelegatingHandler
    {
        public StreamingHttpHandler(HttpMessageHandler innerHandler) : base(innerHandler)
        {
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.SetBrowserResponseStreamingEnabled(true);
            return base.SendAsync(request, cancellationToken);
        }
    }
}
