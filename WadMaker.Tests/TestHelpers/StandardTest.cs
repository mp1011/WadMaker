using WadMaker.Tests.Services;

namespace WadMaker.Tests.TestHelpers;

public class StandardTest
{
    protected virtual int ConfigVersion { get; } = 0;

    protected ServiceProvider ServiceProvider { get; }

    protected Query Query => ServiceProvider.GetRequiredService<Query>();

    protected MapPainter MapPainter => ServiceProvider.GetRequiredService<MapPainter>();

    protected MapBuilder MapBuilder => ServiceProvider.GetRequiredService<MapBuilder>();
    protected RoomBuilder RoomBuilder => ServiceProvider.GetRequiredService<RoomBuilder>();
    protected TextureAdjuster TextureAdjuster => ServiceProvider.GetRequiredService<TextureAdjuster>();

    protected StructureGenerator StructureGenerator => ServiceProvider.GetRequiredService<StructureGenerator>();

    // for compatability with older tests
    protected StructureGenerator HallGenerator => ServiceProvider.GetRequiredService<StructureGenerator>();

    protected ThingPlacer ThingPlacer => ServiceProvider.GetRequiredService<ThingPlacer>();
    protected IDProvider IDProvider => ServiceProvider.GetRequiredService<IDProvider>();

    protected Random Random => ServiceProvider.GetRequiredService<Random>();

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

    /// <summary>
    ///  legacy support for old tests
    /// </summary>
    /// <param name="map"></param>
    public string MapToUDMF_Simple(Map map)
    {
        var elements = MapBuilder.Build(map);
        TextureAdjuster.ApplyTextures(elements);
        return MapPainter.Paint(elements);
    }

    public string MapToUDMF(Map map, bool ensureThingsInBounds = false, bool addPlayerStart=false)
    {
        if (addPlayerStart)
            ThingPlacer.AddPlayerStartToFirstRoomCenter(map);

        var mapElements = MapBuilder.Build(map);
        TextureAdjuster.ApplyTextures(mapElements);
        TextureAdjuster.ApplyThemes(mapElements);
        TextureAdjuster.AdjustOffsetsAndPegs(mapElements);

        if(ensureThingsInBounds)
            OutOfBoundsThingResolver.EnsureThingsAreInBounds(map, mapElements);
        
        return MapPainter.Paint(mapElements);
    }
}
