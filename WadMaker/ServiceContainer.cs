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
        services.AddSingleton<HallGenerator>();
        services.AddSingleton<OverlappingLinedefResolver>();
        services.AddSingleton<MapPainter>();
        services.AddSingleton<IsPointInSector>();
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
        services.AddSingleton<RoomPositionResolver>();

        if (ConfigVersion == 0)
            services.AddSingleton<IConfig, ConfigV0>();
        else if (ConfigVersion == 1)
            services.AddSingleton<IConfig, ConfigV1>();
    }

    public static ServiceProvider CreateServiceProvider(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        configure(services);
        return services.BuildServiceProvider();
    }
}