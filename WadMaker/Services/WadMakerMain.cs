namespace WadMaker.Services;

public class WadMakerMain
{
    public WadMakerMain(MapBuilder mapBuilder, TextureAdjuster textureAdjuster, ThingPlacer thingPlacer, Query query, MapPainter mapPainter, 
        StructureGenerator structureGenerator, OutOfBoundsThingResolver outOfBoundsThingResolver)
    {
        MapBuilder = mapBuilder;
        TextureAdjuster = textureAdjuster;
        ThingPlacer = thingPlacer;
        Query = query;
        MapPainter = mapPainter;
        StructureGenerator = structureGenerator;
        OutOfBoundsThingResolver = outOfBoundsThingResolver;
    }

    public MapBuilder MapBuilder { get; init; }
    public TextureAdjuster TextureAdjuster { get; init; }
    public ThingPlacer ThingPlacer { get; init; }
    public Query Query { get; init; }
    public MapPainter MapPainter { get; init; }
    public StructureGenerator StructureGenerator { get; init; }
    public OutOfBoundsThingResolver OutOfBoundsThingResolver { get; init; }

    public string MapToUdmf(Map map)
    {
        var mapElements = MapBuilder.Build(map);
        TextureAdjuster.ApplyTextures(mapElements);
        TextureAdjuster.ApplyThemes(mapElements);
        TextureAdjuster.AdjustOffsetsAndPegs(mapElements);
        OutOfBoundsThingResolver.EnsureThingsAreInBounds(map, mapElements);
        return MapPainter.Paint(mapElements);
    }

}
