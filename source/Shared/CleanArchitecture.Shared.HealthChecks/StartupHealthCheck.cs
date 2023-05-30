using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Shared.HealthChecks
{
    public class StartupHealthCheck : IHealthCheck
    {
        private readonly IServiceProvider _serviceProvider;

        public StartupHealthCheck(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var hostApplicationLifetimeEventsHostedService =
                _serviceProvider.GetService<HostApplicationLifetimeEventsHostedService>();

            HealthCheckResult result;
            if (hostApplicationLifetimeEventsHostedService.ServiceStatus.State == ServiceState.Started)
            {
                result = HealthCheckResult.Healthy();
            }
            else
            {
                result = HealthCheckResult.Unhealthy("Service not started.");
            }

            return result;
        }
    }
}
