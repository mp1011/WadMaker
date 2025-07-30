using WadMaker.Queries;

namespace WadMaker.Tests.Services;

public static class TestServiceContainer
{
    public static ServiceProvider CreateWithTestAnnotator()
    {
        return ServiceContainer.CreateServiceProvider(services =>
        {
            services.AddSingleton<IDProvider>();
            services.AddSingleton<IAnnotator, TestAnnotator>();
            services.AddSingleton<RoomBuilder>();
            services.AddSingleton<MapBuilder>();
            services.AddSingleton<HallGenerator>();
            services.AddSingleton<OverlappingLinedefResolver>();
            services.AddSingleton<MapPainter>();
            services.AddSingleton<IsPointInSector>();
            services.AddSingleton<RoomGenerator>();
            services.AddSingleton<TextureAdjuster>();
        });
    }

}
