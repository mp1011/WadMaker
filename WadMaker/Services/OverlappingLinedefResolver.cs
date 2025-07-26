namespace WadMaker.Services;

public class OverlappingLinedefResolver
{
    private IAnnotator _annotator;
    private IsPointInSector _isPointInSector;

    public OverlappingLinedefResolver(IAnnotator annotator, IsPointInSector isPointInSector)
    {
        _annotator = annotator;
        _isPointInSector = isPointInSector;
    }

    public IEnumerable<LineDef> Execute(MapElements mapElements)
    {
        List<LineDef> modified = new List<LineDef>();

        int maxIterations = 100000; // safeguard against infinite loops
        while (--maxIterations > 0)
        {
            var nextOverlap = NextOverlappingPair(mapElements);
            if (nextOverlap == null)
                break;
            else
            {
                var modifiedLines = ResolveOverlappingPair(nextOverlap.Value.Item1, nextOverlap.Value.Item2, mapElements).ToArray();
                RemoveOverlappingLines(nextOverlap.Value.Item1, nextOverlap.Value.Item2, mapElements);

                mapElements.LineDefs.AddRange(modifiedLines);

                var sideDefs = modifiedLines.SelectMany(l => l.SideDefs).ToArray();
                mapElements.SideDefs.AddRange(sideDefs.Where(s=> !mapElements.SideDefs.Contains(s)));   

                modified.AddRange(modifiedLines);
            }
        }

        return modified;
    }

    private void RemoveOverlappingLines(LineDef line1, LineDef line2, MapElements mapElements)
    {
        mapElements.LineDefs.Remove(line1);
        mapElements.LineDefs.Remove(line2);
        mapElements.SideDefs.Remove(line1.Front);
        mapElements.SideDefs.Remove(line2.Front);

        if (line1.Back != null)
            mapElements.SideDefs.Remove(line1.Back);

        if (line2.Back != null)
            mapElements.SideDefs.Remove(line2.Back);
    }

    private (LineDef, LineDef)? NextOverlappingPair(MapElements mapElements)
    {
        int index = 0;
        foreach (var lineDef in mapElements.LineDefs)
        {
            var overlap = mapElements.LineDefs.Skip(index + 1)
                .FirstOrDefault(p => p.Overlaps(lineDef));

            if (overlap != null)            
                return (lineDef, overlap);
            
            index++;
        }

        return null;
    }

    public static bool WtfMate(LineDef line)
    {
        return line.Vertices.All(p => p.y == -96 && (p.x == 210 || p.x == 240));
    }

    private IEnumerable<LineDef> ResolveOverlappingPair(LineDef line1, LineDef line2, MapElements mapElements)
    {
        var possibleSectors = line1.Sectors.Union(line2.Sectors).Distinct().ToArray();
        var sourceSidedefs = line1.SideDefs.Union(line2.SideDefs).ToArray();
        var line1Texture = new TextureInfo(line1);
        var line2Texture = new TextureInfo(line2);

        var splitLines = SplitOverlappingLines_IgnoreSidedefs(line1, line2).ToArray();
        foreach(var line in splitLines)
        {
            if (WtfMate(line))
                Console.WriteLine("!");

            Sector? frontSector = possibleSectors.FirstOrDefault(p => _isPointInSector.Execute(line.FrontTestPoint, p, mapElements));
            Sector? backSector = possibleSectors.FirstOrDefault(p => _isPointInSector.Execute(line.BackTestPoint, p, mapElements));

            var frontSidedef = sourceSidedefs.FirstOrDefault(p => p.Sector == frontSector)?.Copy();
            var backSidedef = sourceSidedefs.FirstOrDefault(p => p.Sector == backSector)?.Copy();

            LineDef newLine;

            // each side points to a different sector. This is a two sided line
            if (frontSidedef != backSidedef && frontSidedef != null && backSidedef != null)
            {
                newLine = new LineDef(line.V1, line.V2, frontSidedef, backSidedef, line1.Data with { twosided = true, blocking = false });
            }
            else if (backSector == null && frontSector != null) // single sided linedef
            {
                newLine = new LineDef(line.V1, line.V2, frontSidedef!, null, line1.Data with { twosided = null, blocking = true });
            }
            else if(frontSector == null && backSector == null) // unable to detect sectors, do not include this line
            {
                continue;
            }
            else if(frontSector == null && backSector != null ) // single sided line facing the wrong direction
            {
                newLine = new LineDef(line.V2, line.V1, backSidedef!, null, line1.Data with { twosided = null, blocking = true });
            }
            else
                throw new NotImplementedException("Unexpected overlapping linedef configuration");

            if (line2.Front.Sector == frontSector)
                line2Texture.ApplyTo(newLine);
            else
                line1Texture.ApplyTo(newLine);
            yield return newLine;
        }
    }

    private IEnumerable<LineDef> SplitOverlappingLines_IgnoreSidedefs(LineDef line1, LineDef line2)
    {
        var overlappingVertices = line1.OverlappingVertices(line2);
        var allVertices = line1.Vertices.Union(line2.Vertices).ToArray();

        // pick a vertex, not in the middle, to start
        var initialVertex = allVertices.Except(overlappingVertices).FirstOrDefault() ?? allVertices.First();
        var orderedVertices = allVertices.OrderBy(v => v.SquaredDistanceTo(initialVertex)).ToArray();

        var previousVertex = initialVertex;
        foreach (var vertex in orderedVertices.Skip(1))
        {
            // sidedef arbitrary chosen and will be replaced
            yield return new LineDef(previousVertex, vertex, line1.Front, null, new linedef());
            previousVertex = vertex;
        }

    }
}
