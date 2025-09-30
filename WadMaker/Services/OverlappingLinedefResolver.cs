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
        MapElements originalMapElements = CloneOriginalElements(mapElements);
        List<LineDef> modified = new List<LineDef>();

        int maxIterations = 100000; // safeguard against infinite loops
        while (--maxIterations > 0)
        {
            var nextOverlap = NextOverlappingPair(mapElements);
            if (nextOverlap == null)
                break;
            else
            {
                var modifiedLines = ResolveOverlappingPair(nextOverlap.Value.Item1, nextOverlap.Value.Item2, originalMapElements).ToArray();

                if (!modifiedLines.Any())
                    throw new Exception("Unable to resolve overlapping linedefs");

                RemoveOverlappingLines(nextOverlap.Value.Item1, nextOverlap.Value.Item2, mapElements);

                mapElements.LineDefs.AddRange(modifiedLines);

                var sideDefs = modifiedLines.SelectMany(l => l.SideDefs).ToArray();
                mapElements.SideDefs.AddRange(sideDefs.Where(s => !mapElements.SideDefs.Contains(s)));

                modified.AddRange(modifiedLines);                
            }
        }

        return modified;
    }

    private MapElements CloneOriginalElements(MapElements mapElements)
    {
        return new MapElements
        {
            LineDefs = mapElements.LineDefs.Select(
                p => new LineDef(p.V1, p.V2, p.Front, p.Back, p.Data)).ToList(),
            SideDefs = mapElements.SideDefs.ToList(),
            Sectors = mapElements.Sectors.ToList(),
            Things = mapElements.Things.ToList(),
            Vertices = mapElements.Vertices.ToList()
        };
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

    private IEnumerable<LineDef> ResolveOverlappingPair(LineDef line1, LineDef line2, MapElements originalMapElements)
    {
        var possibleSectors = line1.Sectors.Union(line2.Sectors).Distinct().OrderBy(p=> originalMapElements.Sectors.IndexOf(p)).ToArray();
        var sourceSidedefs = line1.SideDefs.Union(line2.SideDefs).ToArray();
        var line1Texture = new TextureInfo(line1);
        var line2Texture = new TextureInfo(line2);

        if (AreLinesFromCollapsedSector(line1, line2))
        {
            // find the non-collapsed sector
            var sector = possibleSectors.FirstOrDefault(p => p.Room.Bounds.Area > 0);
            if (sector != null)
            {
                var frontSidedef = sourceSidedefs.FirstOrDefault(p => p.Sector == sector)?.Copy();
                if (frontSidedef != null)
                {
                    var newLine = new LineDef(line1.V1, line1.V2, frontSidedef, null, line1.Data with { twosided = null, blocking = true });
                    line1Texture.ApplyTo(newLine);
                    newLine.LineSpecial = line1.LineSpecial ?? line2.LineSpecial;
                    yield return newLine;
                    yield break;
                }
            }
        }


        var splitLines = SplitOverlappingLines_IgnoreSidedefs(line1, line2).ToArray();
        foreach(var line in splitLines)
        {
            // using "last" because we want the most recently added sector to take precence
            Sector? frontSector = possibleSectors.LastOrDefault(p => _isPointInSector.Execute(line.FrontTestPoint, p, originalMapElements));
            Sector? backSector = possibleSectors.LastOrDefault(p => p != frontSector && _isPointInSector.Execute(line.BackTestPoint, p, originalMapElements));

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

            var overlapsOriginalLine1 = newLine.Overlaps(line1);
            var overlapsOriginalLine2 = newLine.Overlaps(line2);


            if (line1.SideDefs.Union(line2.SideDefs).Any(p => p.Texture == Texture.DOORRED.ToString()))
                Console.WriteLine("!");

            throw new Exception("still need to be smarter here");
            if (overlapsOriginalLine1 && !overlapsOriginalLine2)
                line1Texture.ApplyTo(newLine);
            else if(!overlapsOriginalLine1 && overlapsOriginalLine2)
                line2Texture.ApplyTo(newLine);
            else if (line2.Front.Sector == frontSector)
                line2Texture.ApplyTo(newLine);
            else
                line1Texture.ApplyTo(newLine);

            newLine.LineSpecial = line1.LineSpecial ?? line2.LineSpecial;
            yield return newLine;
        }
    }

    /// <summary>
    /// Determiens if the two lines overlap because they belong to a zero-area sector
    /// </summary>
    /// <param name="line1"></param>
    /// <param name="line2"></param>
    /// <returns></returns>
    private bool AreLinesFromCollapsedSector(LineDef line1, LineDef line2)
    {
        if (line1.Length != line2.Length)
            return false;

        var line1Vertices = line1.Vertices.OrderBy(p => p.x).ThenBy(p => p.y).ToArray();
        var line2Vertices = line1.Vertices.OrderBy(p => p.x).ThenBy(p => p.y).ToArray();

        if (line1Vertices[0] != line2Vertices[0] || line1Vertices[1] != line2Vertices[1])
            return false;

        return line1.Front.Sector == line2.Front.Sector;
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
