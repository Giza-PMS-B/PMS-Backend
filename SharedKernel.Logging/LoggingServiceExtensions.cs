using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace SharedKernel.Logging;

public static class LoggingServiceExtensions
{
    public static IServiceCollection AddSharedKernelLogging(this IServiceCollection services)
    {
        // Register any additional logging services here if needed
        return services;
    }

    public static IApplicationBuilder UseSharedKernelLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<LoggingMiddleware>();
        return app;
    }
}