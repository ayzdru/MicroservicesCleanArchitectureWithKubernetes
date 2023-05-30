using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Shared.HealthChecks
{
    public enum ServiceState
    {
        Shutdown,
        Stopped,
        Started
    }

    public class ServiceStatus
    {
        public ServiceState State = ServiceState.Stopped;
    }
}
