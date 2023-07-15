
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace Consul.AspNetCore
{

    public class AgentServiceRegistrationWithDynamicPortHostedService : IHostedService
    {
        private readonly IServer _server;
        private readonly IConsulClient _consulClient;
        private readonly AgentServiceRegistration _serviceRegistration;
        private readonly ILogger _log;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        public AgentServiceRegistrationWithDynamicPortHostedService(ILoggerFactory loggerFactory,
            IHostApplicationLifetime hostApplicationLifetime,
            IServer server,
            IConsulClient consulClient,
            AgentServiceRegistration serviceRegistration)
        {
            _log = loggerFactory.CreateLogger<AgentServiceRegistrationWithDynamicPortHostedService>();
            _hostApplicationLifetime = hostApplicationLifetime;
            _server = server;
            _consulClient = consulClient;
            _serviceRegistration = serviceRegistration;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _hostApplicationLifetime.ApplicationStarted.Register(() => {
                _log.LogInformation("Registering with Consul.");
                var serverAddressesFeature = _server.Features.Get<IServerAddressesFeature>();
                _log.LogInformation("Number of Uris: {@NumberOfUris}", serverAddressesFeature.Addresses.Count);
                var address = serverAddressesFeature.Addresses.FirstOrDefault();
                var uri = new Uri(address);

                _serviceRegistration.Address = uri.Host;
                _serviceRegistration.Port = uri.Port;
                _log.LogInformation("Reporting as bound to port: {@port}", _serviceRegistration.Port);

                _log.LogInformation("Updating endpoints, replacing {port} with actual port number.");
                if (_serviceRegistration.Check != null)
                {
                    _serviceRegistration.Check.HTTP = (_serviceRegistration.Check.HTTP ?? "").Replace("{port}", _serviceRegistration.Port.ToString());
                    _serviceRegistration.Check.TCP = (_serviceRegistration.Check.TCP ?? "").Replace("{port}", _serviceRegistration.Port.ToString());
                }
                foreach (var x in _serviceRegistration.Checks ?? new AgentServiceCheck[0])
                {
                    x.HTTP = (x.HTTP ?? "").Replace("{port}", _serviceRegistration.Port.ToString());
                    x.TCP = (x.TCP ?? "").Replace("{port}", _serviceRegistration.Port.ToString());
                }

                _consulClient.Agent.ServiceRegister(_serviceRegistration, CancellationToken.None).GetAwaiter().GetResult();
            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _consulClient.Agent.ServiceDeregister(_serviceRegistration.ID, cancellationToken);
        }
    }

    public static class AdditionalServiceCollectionExtensions
    {
        public static IServiceCollection AddConsulServiceRegistrationWithDynamicPort(this IServiceCollection services, Action<AgentServiceRegistration> configure)
        {
            var registration = new AgentServiceRegistration();

            configure.Invoke(registration);

            return services
                .AddSingleton(registration)
                .AddHostedService<AgentServiceRegistrationWithDynamicPortHostedService>();
        }
    }
}