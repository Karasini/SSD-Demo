using System.Reflection;

namespace Transcriptor.Api.Infrastructure.DI;

public static class CommandRegistration
{
    public static IServiceCollection AddCommands(this IServiceCollection services, Assembly assembly)
    {
        RegisterMarkerImplementations(services, assembly, typeof(ICommand), "Command");
        return services;
    }

    private static void RegisterMarkerImplementations(
        IServiceCollection services,
        Assembly assembly,
        Type marker,
        string suffix)
    {
        var types = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false }
                        && t.GetInterfaces().Any(i =>
                            i != marker
                            && i.Name.EndsWith(suffix, StringComparison.Ordinal)));

        foreach (var type in types)
        {
            var iface = type.GetInterfaces().First(i =>
                i != marker && i.Name.EndsWith(suffix, StringComparison.Ordinal));
            services.AddScoped(iface, type);
        }
    }
}
