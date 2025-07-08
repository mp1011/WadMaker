namespace WadMaker.Services;

public class OverlappingLinedefResolver
{
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
                mapElements.LineDefs.Remove(nextOverlap.Value.Item1);
                mapElements.LineDefs.Remove(nextOverlap.Value.Item2);
                mapElements.SideDefs.Remove(nextOverlap.Value.Item1.Front);
                mapElements.SideDefs.Remove(nextOverlap.Value.Item2.Front);

                var modifiedLines = ResolveOverlappingPair(nextOverlap.Value.Item1, nextOverlap.Value.Item2).ToArray();
                mapElements.LineDefs.AddRange(modifiedLines);

                var sideDefs = modifiedLines.SelectMany(l => l.SideDefs).ToArray();
                mapElements.SideDefs.AddRange(sideDefs.Where(s=> !mapElements.SideDefs.Contains(s)));   

                modified.AddRange(modifiedLines);
            }
        }

        return modified;
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

    private IEnumerable<LineDef> ResolveOverlappingPair(LineDef line1, LineDef line2)
    {
        var overlappingVertices = line1.OverlappingVertices(line2);

        var allVertices = line1.Vertices.Union(line2.Vertices).ToArray();

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
                // todo, deal with vertex indices?
                yield return new LineDef(previous, vertex, previousLine.Front.Copy(), null, previousLine.Data);
            }
            else if (index == allVertices.Length - 1) // last segment
            {
                yield return new LineDef(previous, vertex, thisLine.Front.Copy(), null, thisLine.Data);
            }
            else // overlapping middle segments 
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

                    new linedef(blocking:false, twoSided:true, comment: "double sided line"));               
            }

            previous = vertex;
            index++;
        }
    }
}
