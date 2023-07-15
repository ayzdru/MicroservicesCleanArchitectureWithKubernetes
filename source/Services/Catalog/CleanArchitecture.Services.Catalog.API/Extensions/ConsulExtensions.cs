using Microsoft.Extensions.DependencyInjection;
using System;

namespace Consul.AspNetCore
{
    public static class ConsulExtensions
    {
        public static IServiceCollection AddConsulDynamicServiceRegistration(
            this IServiceCollection services,
            Action<AgentServiceRegistration> configure)
        {
            var registration = new AgentServiceRegistration();

            configure.Invoke(registration);

            return services
                .AddSingleton(registration)
                .AddHostedService<AgentServiceRegistrationWithDynamicPortHostedService>();
        }
    }
}
