using WadMaker.Tests.Fixtures;
using WadMaker.Tests.Services;

namespace WadMaker.Tests.TestHelpers;

internal class StandardTest
{
    protected virtual int ConfigVersion { get; } = 0;

    protected ServiceProvider ServiceProvider { get; }

    protected Query Query => ServiceProvider.GetRequiredService<Query>();

    protected MapPainter MapPainter => ServiceProvider.GetRequiredService<MapPainter>();

    protected MapBuilder MapBuilder => ServiceProvider.GetRequiredService<MapBuilder>();
    protected RoomBuilder RoomBuilder => ServiceProvider.GetRequiredService<RoomBuilder>();
    protected TextureAdjuster TextureAdjuster => ServiceProvider.GetRequiredService<TextureAdjuster>();

    protected RoomGenerator RoomGenerator => ServiceProvider.GetRequiredService<RoomGenerator>();
    protected HallGenerator HallGenerator => ServiceProvider.GetRequiredService<HallGenerator>();
    protected ThingPlacer ThingPlacer => ServiceProvider.GetRequiredService<ThingPlacer>();
    protected IDProvider IDProvider => ServiceProvider.GetRequiredService<IDProvider>();

    protected OutOfBoundsThingResolver OutOfBoundsThingResolver => ServiceProvider.GetRequiredService<OutOfBoundsThingResolver>();

    public StandardTest()
    {
        ServiceContainer.ConfigVersion = ConfigVersion;
        ServiceProvider = TestServiceContainer.CreateWithTestAnnotator();
    }

    public Point[] IntArrayToPointList(int[] array)
    {
        if (array == null || array.Length % 2 != 0)
            throw new ArgumentException("Array must contain an even number of elements.");

        var points = new Point[array.Length / 2];
        for (int i = 0; i < array.Length; i += 2)
        {
            points[i / 2] = new Point(array[i], array[i + 1]);
        }
        return points;
    }

    public string MapToUDMF(Map map)
    {
        var mapElements = MapBuilder.Build(map);
        TextureAdjuster.ApplyThemes(mapElements);
        TextureAdjuster.AdjustOffsetsAndPegs(mapElements);
        return MapPainter.Paint(mapElements);
    }
}
