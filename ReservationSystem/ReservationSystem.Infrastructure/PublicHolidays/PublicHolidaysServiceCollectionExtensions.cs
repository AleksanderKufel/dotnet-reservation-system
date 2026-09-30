using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ReservationSystem.Application.Interfaces;

namespace ReservationSystem.Infrastructure.PublicHolidays;

public static class PublicHolidaysServiceCollectionExtensions
{
    public static IServiceCollection AddPublicHolidays(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<PublicHolidaysOptions>()
            .Bind(configuration.GetSection(PublicHolidaysOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<NagerDateClient>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<PublicHolidaysOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
            })
            // Retries with backoff, per-attempt and total timeouts, circuit breaker.
            // Timeouts are short because a user waits for the reservation response.
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 2;
                options.Retry.Delay = TimeSpan.FromMilliseconds(200);
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(3);
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(10);
            });

        services.AddMemoryCache();

        services.AddScoped<IPublicHolidayProvider, PublicHolidayProvider>();

        return services;
    }
}
