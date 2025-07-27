using WadMaker.Tests.Services;

namespace WadMaker.Tests.TestHelpers;

internal class StandardTest
{
    protected ServiceProvider ServiceProvider { get; }

    protected MapPainter MapPainter => ServiceProvider.GetRequiredService<MapPainter>();

    protected MapBuilder MapBuilder => ServiceProvider.GetRequiredService<MapBuilder>();
    protected RoomBuilder RoomBuilder => ServiceProvider.GetRequiredService<RoomBuilder>();


    protected RoomGenerator RoomGenerator => ServiceProvider.GetRequiredService<RoomGenerator>();


    public StandardTest()
    {
        ServiceProvider = TestServiceContainer.CreateWithTestAnnotator();
    }
}
