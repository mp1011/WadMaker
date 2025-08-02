using WadMaker.Tests.Fixtures;
using WadMaker.Tests.Services;

namespace WadMaker.Tests.TestHelpers;

internal class StandardTest
{
    protected ServiceProvider ServiceProvider { get; }

    protected MapPainter MapPainter => ServiceProvider.GetRequiredService<MapPainter>();

    protected MapBuilder MapBuilder => ServiceProvider.GetRequiredService<MapBuilder>();
    protected RoomBuilder RoomBuilder => ServiceProvider.GetRequiredService<RoomBuilder>();
    protected TextureAdjuster TextureAdjuster => ServiceProvider.GetRequiredService<TextureAdjuster>();

    protected RoomGenerator RoomGenerator => ServiceProvider.GetRequiredService<RoomGenerator>();
    protected HallGenerator HallGenerator => ServiceProvider.GetRequiredService<HallGenerator>();


    public StandardTest()
    {
        ServiceProvider = TestServiceContainer.CreateWithTestAnnotator();
    }

    public string MapToUDMF(Map map) =>
        MapPainter.Paint(TextureAdjuster.AdjustOffsetsAndPegs(MapBuilder.Build(map)));
}
