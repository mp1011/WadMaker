using System.Reflection;
using WadMaker.Services.StructureGenerators;

namespace WadMaker.Services;

public static class ServiceContainer
{
    public static int ConfigVersion = 0;

    public static void StandardDependencies(IServiceCollection services)
    {
        services.AddSingleton<IDProvider>();
        services.AddSingleton<IAnnotator, EmpyAnnotator>();
        services.AddSingleton(Random.Shared);
        services.AddSingleton<RoomBuilder>();
        services.AddSingleton<MapBuilder>();
        services.AddSingleton<OverlappingLinedefResolver>();
        services.AddSingleton<MapPainter>();
        services.AddSingleton<IsPointInSector>();
        services.AddSingleton<DoorColorBarGenerator>();
        services.AddSingleton<StructureGenerator>();
        services.AddSingleton<TextureAdjuster>();
        services.AddSingleton<ThingPlacer>();
        services.AddSingleton<OverlappingThingResolver>();
        services.AddSingleton<OutOfBoundsThingResolver>();
        services.AddSingleton<Query>();

        services.AddSingleton<CountAmmoAmounts>();
        services.AddSingleton<CountMeanWeaponDamage>();
        services.AddSingleton<CountMonsterHp>();
        services.AddSingleton<GetAmmoBalance>();
        services.AddSingleton<IsPointInSector>();
        services.AddSingleton<GetThingSector>();
        services.AddSingleton<IsThingInBounds>();
        services.AddSingleton<IsOverlappingAnotherThingOfSameCategory>();
        services.AddSingleton<TextureQuery>();
        services.AddSingleton<DoorColorBarGenerator>();

        RegisterAllGenericInterfaceImplementations(services, typeof(IStructureGenerator<>), Assembly.GetExecutingAssembly());
        RegisterAllGenericInterfaceImplementations(services, typeof(ISideStructureGenerator<>), Assembly.GetExecutingAssembly());
        RegisterAllGenericInterfaceImplementations(services, typeof(IMultiRoomStructureGenerator<>), Assembly.GetExecutingAssembly());

        if (ConfigVersion == 0)
            services.AddSingleton<IConfig, ConfigV0>();
        else if (ConfigVersion == 1)
            services.AddSingleton<IConfig, ConfigV1>();
    }

    /// <summary>
    /// Registers all non-abstract, non-interface types in the given assembly that implement the given open generic interface.
    /// </summary>
    public static void RegisterAllGenericInterfaceImplementations(IServiceCollection services, Type openGenericInterface, Assembly assembly)
    {
        var types = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface);

        foreach (var type in types)
        {
            var interfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericInterface);

            foreach (var iface in interfaces)
            {
                services.AddSingleton(iface, type);
            }
        }
    }

    public static ServiceProvider CreateServiceProvider(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        configure(services);
        return services.BuildServiceProvider();
    }
}