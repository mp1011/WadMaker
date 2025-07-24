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

    private IEnumerable<LineDef> ResolveOverlappingPair(LineDef line1, LineDef line2, MapElements mapElements) 
    {
        var overlappingVertices = line1.OverlappingVertices(line2);

        var allVertices = line1.Vertices.Union(line2.Vertices).ToArray();

        if (allVertices.Length > overlappingVertices.Length)
        {
            return ResolvePartiallyOverlappingPair(line1, line2, allVertices, overlappingVertices);
        }
        else
        {
            return [ResolveFullyOverlappingPair(line1, line2, overlappingVertices, mapElements)];
        }
    }

    private IEnumerable<LineDef> ResolvePartiallyOverlappingPair(LineDef line1, LineDef line2, vertex[] allVertices, vertex[] overlappingVertices)
    { 
        // pick one of the non-overlapping vertices to start
        var initialVertex = allVertices.Except(overlappingVertices).First();

        allVertices = allVertices.OrderBy(v => v.SquaredDistanceTo(initialVertex)).ToArray();

        int index = 1;
        var previous = initialVertex;
        var initialVertexSector = line1.Contains(initialVertex) ? line1.Front.Sector : line2.Front.Sector;
        var otherSector = initialVertexSector == line1.Front.Sector ? line2.Front.Sector : line1.Front.Sector;

        foreach (var vertex in allVertices.Skip(1))
        {
            var previousLine = line1.Contains(previous) ? line1 : line2;
            var thisLine = line1.Contains(vertex) ? line1 : line2;

            if (index == 1) // first segment
            {
                yield return new LineDef(previous, vertex, previousLine.Front.Copy(), null, previousLine.Data);
            }
            else if (index == allVertices.Length - 1 && allVertices.Length != 3) // last segment. Need to handle this better.
            {
                yield return new LineDef(previous, vertex, thisLine.Front.Copy(), null, thisLine.Data);
            }
            else if (initialVertexSector != otherSector)
            {
                // overlapping middle segments             
                if (line1.FrontAngle == line2.FrontAngle)
                {
                    //combine into one single sided line
                    var copyFromLine = new[] { line1, line2 }.OrderBy(p => p.Length).First();
                    var targetSector = copyFromLine.Front.Sector;
                    var targetTexture = new TextureInfo(copyFromLine);

                    yield return new LineDef(previous, vertex,
                        new SideDef(targetSector, new sidedef(
                            sector: -1,                            
                            texturemiddle: previousLine.Front.Data.texturetop)),
                            null,
                            new linedef(blocking: true).AddComment(null, _annotator)).ApplyTexture(targetTexture);
                }
                else
                { 
                    yield return new LineDef(previous, vertex,
                        new SideDef(initialVertexSector, new sidedef(
                            sector: -1,
                            texturemiddle: null,
                            texturetop: previousLine.Front.Data.texturemiddle,
                            texturebottom: previousLine.Front.Data.texturemiddle)),
                        new SideDef(otherSector, new sidedef(
                            sector: -1,
                            texturemiddle: null,
                            texturetop: previousLine.Front.Data.texturemiddle,
                            texturebottom: previousLine.Front.Data.texturemiddle)),

                        new linedef(blocking: false, twosided: true).AddComment(null, _annotator));
                }
            }

            previous = vertex;
            index++;
        }
    }
   
    private LineDef ResolveFullyOverlappingPair(LineDef line1, LineDef line2, vertex[] overlappingVertices, MapElements mapElements)
    {
        var possibleSectors = line1.Sectors.Union(line2.Sectors).Distinct().ToArray();

        // TODO: what if front is not found?
        Sector frontSector = possibleSectors.First(p => _isPointInSector.Execute(line1.FrontTestPoint, p, mapElements));
        Sector? backSector = possibleSectors.FirstOrDefault(p => _isPointInSector.Execute(line1.BackTestPoint, p, mapElements));

        TextureInfo texture = new TextureInfo(line1);

        var newLine = new LineDef(overlappingVertices[0], overlappingVertices[1],
                        new SideDef(frontSector, new sidedef()),
                        backSector == null ? null : new SideDef(backSector, new sidedef()),
                        new linedef(blocking: backSector == null, twosided: backSector != null));

        return newLine.ApplyTexture(texture);
    }
}
