using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WordInverser.Common.Interfaces;
using WordInverser.DAL.Context;
using WordInverser.DAL.Interfaces;
using WordInverser.DAL.Repositories;

namespace WordInverser.DAL;

public class DALServiceRegistration : IServiceRegistration
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<WordInverserDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }));

        // Register Repositories
        services.AddScoped<IWordCacheRepository, WordCacheRepository>();
        services.AddScoped<IRequestResponseRepository, RequestResponseRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}
