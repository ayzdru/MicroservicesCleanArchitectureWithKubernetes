using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;

namespace CleanArchitecture.Shared.HealthChecks
{
    public static class HealthCheckExtensions
    {
        public static readonly TimeSpan DefaultTimeOut = TimeSpan.FromSeconds(3);
        public const string Startup = "start";
        public const string Liveness = "liveness";
        public const string Readiness = "readiness ";
        public static IHealthChecksBuilder AddAllHealthChecks(this IServiceCollection services)
        {
            services.AddSingleton<HostApplicationLifetimeEventsHostedService>();
            services.AddHostedService(p => p.GetRequiredService<HostApplicationLifetimeEventsHostedService>());
            var healthchecks = services.AddHealthChecks();
            healthchecks
               .AddCheck<StartupHealthCheck>("Service Startup",
                  HealthStatus.Unhealthy,
                  new[] { Startup }, DefaultTimeOut
                 )
            .AddCheck("Self test",
                () => HealthCheckResult.Healthy(),
                new[] { Liveness },
                DefaultTimeOut);
            return healthchecks;
        }

        public static void UseAllHealthChecks(this IApplicationBuilder app)
        {
            app.UseHealthChecks("/health/startup",
             new HealthCheckOptions { Predicate = check => check.Tags.Contains(HealthCheckExtensions.Startup) });

            app.UseHealthChecks("/health/liveness",
         new HealthCheckOptions { Predicate = check => check.Tags.Contains(HealthCheckExtensions.Liveness) });
            app.UseHealthChecks("/health/readiness",
           new HealthCheckOptions { Predicate = check => check.Tags.Contains(HealthCheckExtensions.Readiness) });

            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
        }
    }
}
