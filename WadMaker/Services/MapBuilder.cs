namespace WadMaker.Services;

public class MapBuilder
{
    private readonly RoomBuilder _roomBuilder;
    private readonly OverlappingLinedefResolver _overlappingLinedefResolver;
    private readonly IsPointInSector _isPointInSector;

    public MapBuilder(RoomBuilder roomBuilder, OverlappingLinedefResolver overlappingLinedefResolver, IsPointInSector isPointInSector)
    {
        _roomBuilder = roomBuilder;
        _overlappingLinedefResolver = overlappingLinedefResolver;
        _isPointInSector = isPointInSector;
    }

    public MapElements Build(Map map)
    {
        var mapElements = new MapElements();
        foreach (var room in map.Rooms)
        {
            var roomElements = _roomBuilder.Build(room);
            mapElements.Merge(roomElements);
        }

        _overlappingLinedefResolver.Execute(mapElements);
        RemoveZeroLengthLines(mapElements);
        RemoveInvalidSidedefs(mapElements);
        EnsureSingleSidedLinedefsAreFacingInward(mapElements);
        EnsureActionLinedefsFacingCorrectDirection(mapElements);
        RemoveUnusedElements(mapElements);

        foreach (var sideDef in mapElements.SideDefs)
        {
            sideDef.Resolve(mapElements);
        }

        foreach (var lineDef in mapElements.LineDefs)
        {
            lineDef.Resolve(mapElements);
        }

        foreach(var sector in mapElements.Sectors)
        {
            sector.Lines = [.. mapElements.LineDefs.Where(p => p.BelongsTo(sector))];

            if (sector.Tag.HasValue)
            {
                sector.Activators = mapElements.LineDefs.Where(p => p.LineSpecial != null
                    && p.LineSpecial.SectorTag == sector.Tag).ToArray();
            }
        }

        return mapElements;
    }

    private void RemoveUnusedElements(MapElements mapElements)
    {
        if (Legacy.Flags.HasFlag(LegacyFlags.DontClearUnusedMapElements))
            return;

        var usedSectors = mapElements.LineDefs.SelectMany(p => p.Sectors).Distinct().ToArray();
        var usedSideDefs = mapElements.LineDefs.SelectMany(p=>p.SideDefs).Distinct().ToArray();
        var usedVertices = mapElements.LineDefs.SelectMany(p => p.Vertices).Distinct().ToArray();

        var unusedSectors = mapElements.Sectors.Except(usedSectors).ToArray();
        var unusedSideDefs = mapElements.SideDefs.Except(usedSideDefs).ToArray();
        var unusedVertices = mapElements.Vertices.Except(usedVertices).ToArray();

        mapElements.Sectors.RemoveMany(unusedSectors);
        mapElements.SideDefs.RemoveMany(unusedSideDefs);
        mapElements.Vertices.RemoveMany(unusedVertices);
    }

    private void EnsureActionLinedefsFacingCorrectDirection(MapElements mapElements)
    {
        foreach(var specialLine in mapElements.LineDefs.Where(p=>p.LineSpecial != null && p.Back != null))
        {
            // note, not able to handle when both sides have specials
            if (specialLine.LineSpecial!.AppliesToBackSector && specialLine.Front.Sector.Room.LineSpecials.Any())
                specialLine.FlipSides();
        }
    }

    private void EnsureSingleSidedLinedefsAreFacingInward(MapElements mapElements)
    {
        foreach(var line in mapElements.LineDefs.Where(p=>p.Back == null))
        {            
            if (!_isPointInSector.Execute(line.FrontTestPoint, line.Front.Sector, mapElements))
            {
                line.FlipDirection();
            }
        }
    }

    private void RemoveZeroLengthLines(MapElements mapElements)
    {
        var invalidLines = mapElements.LineDefs.Where(p => p.Length == 0).ToArray();
        var invalidSides = invalidLines.SelectMany(p => p.SideDefs).ToArray();

        mapElements.LineDefs.RemoveMany(invalidLines);
        mapElements.SideDefs.RemoveMany(invalidSides);
        mapElements.ClearSectorPolygonCache();
    }

    private void RemoveInvalidSidedefs(MapElements mapElements)
    {
        var lineDefsToInspect = mapElements.LineDefs.Where(p => p.Back != null).ToArray();

        var sideDefsToRemove = new ConcurrentBag<SideDef>();
        var linesNeedingBackRemoval = new ConcurrentBag<LineDef>();

        Parallel.ForEach(lineDefsToInspect, lineDef =>  
        {
            Sector? frontSector = mapElements.Sectors.FirstOrDefault(p => _isPointInSector.Execute(lineDef.FrontTestPoint, p, mapElements));
            Sector? backSector = mapElements.Sectors.FirstOrDefault(p => _isPointInSector.Execute(lineDef.BackTestPoint, p, mapElements));

            if (frontSector == null)
            {
                // should do something, but don't know what
            }
            else if (backSector == null)
            {
                var back = lineDef.Back;
                if (back != null)
                    sideDefsToRemove.Add(back);

                linesNeedingBackRemoval.Add(lineDef);
            }
        });

        var distinctSideDefs = sideDefsToRemove.Distinct().ToArray();
        var distinctLines = linesNeedingBackRemoval.Distinct().ToArray();

        if (distinctSideDefs.Length > 0)
            mapElements.SideDefs.RemoveMany(distinctSideDefs);

        foreach (var line in distinctLines)
        {
            line.RemoveBack();
        }

        mapElements.ClearSectorPolygonCache();
    }
}
