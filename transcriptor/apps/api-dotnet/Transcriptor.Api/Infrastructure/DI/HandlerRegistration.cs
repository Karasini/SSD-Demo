using System.Reflection;

namespace Transcriptor.Api.Infrastructure.DI;

public interface IHandler { }

public static class HandlerRegistration
{
    public static IServiceCollection AddHandlers(this IServiceCollection services, Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false }
                        && t.GetInterfaces().Any(i =>
                            i != typeof(IHandler)
                            && i.Name.EndsWith("Handler", StringComparison.Ordinal)));

        foreach (var type in handlerTypes)
        {
            var iface = type.GetInterfaces().First(i =>
                i != typeof(IHandler) && i.Name.EndsWith("Handler", StringComparison.Ordinal));
            services.AddScoped(iface, type);
        }

        return services;
    }
}
