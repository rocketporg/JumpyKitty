using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;

namespace JumpyKitty.Core.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAllImplementationsAsSelf<TBase>(this IServiceCollection services, ServiceLifetime lifetime, Assembly assembly)
    {
        var baseType = typeof(TBase);
        var types = assembly
            .GetTypes()
            .Where(t => baseType.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

        foreach (var type in types)
        {
            var descriptor = new ServiceDescriptor(type, type, lifetime);
            services.Add(descriptor);
        }

        return services;
    }
}
