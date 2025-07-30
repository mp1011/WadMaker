namespace WadMaker.Services;

public static class ServiceContainer
{
    public static void StandardDependencies(IServiceCollection services)
    {
        services.AddSingleton<IDProvider>();
        services.AddSingleton<IAnnotator, EmpyAnnotator>();
        services.AddSingleton<RoomBuilder>();
        services.AddSingleton<MapBuilder>();
        services.AddSingleton<HallGenerator>();
        services.AddSingleton<OverlappingLinedefResolver>();
        services.AddSingleton<MapPainter>();
        services.AddSingleton<IsPointInSector>();
        services.AddSingleton<RoomGenerator>();
        services.AddSingleton<TextureAdjuster>();
    }

    public static void DiagnosticDependencies(IServiceCollection services)
    {
        services.AddSingleton<IDProvider>();
        services.AddSingleton<IAnnotator, VerboseAnnotator>();
        services.AddSingleton<RoomBuilder>();
        services.AddSingleton<MapBuilder>();
        services.AddSingleton<HallGenerator>();
        services.AddSingleton<OverlappingLinedefResolver>();
        services.AddSingleton<MapPainter>();
        services.AddSingleton<IsPointInSector>();
        services.AddSingleton<RoomGenerator>();
        services.AddSingleton<TextureAdjuster>();
    }

    public static ServiceProvider CreateServiceProvider(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        configure(services);
        return services.BuildServiceProvider();
    }
}