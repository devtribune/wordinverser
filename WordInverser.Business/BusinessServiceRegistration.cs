using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WordInverser.Business.Interfaces;
using WordInverser.Business.Services;
using WordInverser.Common.Interfaces;

namespace WordInverser.Business;

public class BusinessServiceRegistration : IServiceRegistration
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register Memory Cache
        services.AddMemoryCache();

        // Register Services
        services.AddScoped<IWordInversionService, WordInversionService>();
        services.AddScoped<IRequestResponseService, RequestResponseService>();
        services.AddSingleton<IWordCacheService, WordCacheService>();
    }
}
