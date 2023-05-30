using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Shared.HealthChecks
{
    public class HostApplicationLifetimeEventsHostedService : IHostedService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        public ServiceStatus ServiceStatus { get; private set; }

        public HostApplicationLifetimeEventsHostedService(IHostApplicationLifetime hostApplicationLifetime)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            ServiceStatus = new ServiceStatus();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _hostApplicationLifetime.ApplicationStarted.Register(OnStarted);
            _hostApplicationLifetime.ApplicationStopping.Register(OnShutdown);
            _hostApplicationLifetime.ApplicationStopped.Register(OnStopped);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
           => Task.CompletedTask;

        private void OnShutdown()
        {
            for (var i = 5; i > 0; i--)
            {
                ServiceStatus.State = ServiceState.Shutdown;
                Thread.Sleep(1000);
            }
        }

        private void OnStopped()
        {
            ServiceStatus.State = ServiceState.Stopped;
        }

        private void OnStarted()
        {
            ServiceStatus.State = ServiceState.Started;
        }
    }
}
