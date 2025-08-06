using WadMaker.Tests.Fixtures;
using WadMaker.Tests.Services;

namespace WadMaker.Tests.TestHelpers;

internal class StandardTest
{
    protected virtual int ConfigVersion { get; } = 0;
    
    protected ServiceProvider ServiceProvider { get; }

    protected MapPainter MapPainter => ServiceProvider.GetRequiredService<MapPainter>();

    protected MapBuilder MapBuilder => ServiceProvider.GetRequiredService<MapBuilder>();
    protected RoomBuilder RoomBuilder => ServiceProvider.GetRequiredService<RoomBuilder>();
    protected TextureAdjuster TextureAdjuster => ServiceProvider.GetRequiredService<TextureAdjuster>();

    protected RoomGenerator RoomGenerator => ServiceProvider.GetRequiredService<RoomGenerator>();
    protected HallGenerator HallGenerator => ServiceProvider.GetRequiredService<HallGenerator>();
    protected ThingPlacer ThingPlacer => ServiceProvider.GetRequiredService<ThingPlacer>();

  public StandardTest()
  {
    ServiceContainer.ConfigVersion = ConfigVersion;
    ServiceProvider = TestServiceContainer.CreateWithTestAnnotator();
  }

    public string MapToUDMF(Map map)
    {
        var mapElements = MapBuilder.Build(map);
        TextureAdjuster.ApplyThemes(mapElements);
        TextureAdjuster.AdjustOffsetsAndPegs(mapElements);
        return MapPainter.Paint(mapElements);
    }
}
