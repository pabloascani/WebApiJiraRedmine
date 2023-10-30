using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading;
using System.Threading.Tasks;

namespace WebApiJiraRedmineApi.Controllers
{
    public class SampleHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var IsHealthy = true;

            if (IsHealthy) return Task.FromResult(HealthCheckResult.Healthy("A healthy result."));

            return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, "An unhealthy result."));
        }
    }
}
