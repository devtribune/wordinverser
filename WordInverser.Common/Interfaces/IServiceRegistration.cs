using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WordInverser.Common.Interfaces;

public interface IServiceRegistration
{
    void RegisterServices(IServiceCollection services, IConfiguration configuration);
}
