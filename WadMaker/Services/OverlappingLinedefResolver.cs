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

        modified.AddRange(ResolveCrossingLines(originalMapElements, mapElements));
        return modified;
    }

    private IEnumerable<LineDef> ResolveCrossingLines(MapElements originalMapElements, MapElements mapElements)
    {
        Dictionary<(Sector, Sector), Sector> overlapSectors = new();

        List<LineDef> modified = new();

        int maxIterations = 100000; // safeguard against infinite loops
        while (--maxIterations > 0)
        {
            var nextCrossing = NextCrossingPair(mapElements);
            if (nextCrossing == null)
                break;
            else
            {
                var modifiedLines = ResolveCrossingPair(nextCrossing.Value.Item1, nextCrossing.Value.Item2, originalMapElements, overlapSectors).ToArray();

                if (!modifiedLines.Any())
                    throw new Exception("Unable to resolve crossing linedefs");

                RemoveOverlappingLines(nextCrossing.Value.Item1, nextCrossing.Value.Item2, mapElements);

                mapElements.LineDefs.AddRange(modifiedLines);

                var sideDefs = modifiedLines.SelectMany(l => l.SideDefs).ToArray();
                mapElements.SideDefs.AddRange(sideDefs.Where(s => !mapElements.SideDefs.Contains(s)));

                var vertices = modifiedLines.SelectMany(p => p.Vertices).Distinct().ToArray();
                mapElements.Vertices.AddRange(vertices.Where(v => !mapElements.Vertices.Contains(v)));

                modified.AddRange(modifiedLines);
            }
        }

        mapElements.Sectors.AddRange(overlapSectors.Values);
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

    private (LineDef, LineDef)? NextCrossingPair(MapElements mapElements)
    {
        int index = 0;
        foreach (var lineDef in mapElements.LineDefs)
        {
            var overlap = mapElements.LineDefs.Skip(index + 1)
                .FirstOrDefault(p => p.Crosses(lineDef));

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
        var line1Texture = line1.TextureInfo;
        var line2Texture = line2.TextureInfo;

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
                    newLine.TextureInfo = line1Texture;
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

            var originalFrontSidedef = sourceSidedefs.FirstOrDefault(p => p.Sector == frontSector);
            var originalBackSidedef = sourceSidedefs.FirstOrDefault(p => p.Sector == backSector);
            var frontSidedef = originalFrontSidedef?.Copy();
            var backSidedef = originalBackSidedef?.Copy();

            var originalFrontLine = new[] { line1, line2 }.FirstOrDefault(p => p.SideDefs.Contains(originalFrontSidedef));
            var originalBackLine = new[] { line1, line2 }.FirstOrDefault(p => p.SideDefs.Contains(originalBackSidedef));


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


            // figure out which texture to apply to each side
            if (overlapsOriginalLine1 && !overlapsOriginalLine2)
                newLine.TextureInfo = line1Texture;
            else if (!overlapsOriginalLine1 && overlapsOriginalLine2)
                newLine.TextureInfo = line2Texture;
            else if (newLine.SingleSided && newLine.InSamePositionAs(line1))
                newLine.TextureInfo = line1Texture;
            else if (newLine.SingleSided && newLine.InSamePositionAs(line2))
                newLine.TextureInfo = line2Texture;
            else
            {
                if (frontSidedef != null)
                    frontSidedef.TextureInfo = originalFrontSidedef!.TextureInfo ?? originalFrontLine!.TextureInfo;

                if (backSidedef != null)
                    backSidedef.TextureInfo = originalBackSidedef!.TextureInfo ?? originalBackLine!.TextureInfo;
            }

            if (overlapsOriginalLine1 && !overlapsOriginalLine2)
                newLine.LineSpecial = line1.LineSpecial;
            else if (!overlapsOriginalLine1 && overlapsOriginalLine2)
                newLine.LineSpecial = line2.LineSpecial;
            else
                newLine.LineSpecial = line1.LineSpecial ?? line2.LineSpecial;

            newLine.BlocksSounds = line1.BlocksSounds || line2.BlocksSounds;
            yield return newLine;
        }
    }

    private IEnumerable<LineDef> ResolveCrossingPair(LineDef line1, LineDef line2, MapElements originalMapElements,
         Dictionary<(Sector, Sector), Sector> overlapSectors)
    {
        var sectors = line1.Sectors.Union(line2.Sectors).Distinct().ToArray();
        var vertices = line1.Vertices.Union(line2.Vertices).ToArray();
        var sides = line1.SideDefs.Union(line2.SideDefs).ToArray();

        var intersectionPoint = line1.IntersectionPoint(line2)!;
        var intersectionVertex = new vertex(intersectionPoint.Value.X, intersectionPoint.Value.Y);

        // create initial lines with arbitrary sidedefs
        var lines = vertices.Select(v => new LineDef(v, intersectionVertex, line1.Front, null, new linedef())).ToArray();

        return lines.Select(line =>
        {
            LineDef originalLine;
            if (line.Angle == line1.Angle.Opposite)
            {
                originalLine = line1;
                line.FlipDirection();
            }
            else if (line.Angle == line1.Angle)
            {
                originalLine = line1;
            }
            else if (line.Angle == line2.Angle.Opposite)
            {
                originalLine = line2;
                line.FlipDirection();
            }
            else
            {
                originalLine = line2;
            }

            var texture = originalLine.TextureInfo;

            var frontSectors = originalMapElements.Sectors.Where(s => _isPointInSector.Execute(line.FrontTestPoint, s, originalMapElements)).ToArray();
            var backSectors = originalMapElements.Sectors.Where(s => _isPointInSector.Execute(line.BackTestPoint, s, originalMapElements)).ToArray();

            if (frontSectors.Length == 1 && backSectors.Length == 0)
            {
                return new LineDef(line.V1, line.V2, originalLine.Front.Copy(), null, originalLine.Data);
            }
            else if(frontSectors.Length == 2 && backSectors.Length == 1)
            {
                var backSide = sides.First(p => p.Sector == backSectors[0]).Copy();

                var overlapSector = overlapSectors.Try((frontSectors[0], frontSectors[1]))
                    ?? overlapSectors.Try((frontSectors[1], frontSectors[0]));

                if(overlapSector == null)
                {
                    overlapSector = new Sector(frontSectors[0].Room, frontSectors[0].Data);
                    overlapSectors[(frontSectors[0], frontSectors[1])] = overlapSector;
                }

                var frontSide = new SideDef(overlapSector, originalLine.Front.Data);
                var newLine = new LineDef(line.V1, line.V2, frontSide, backSide, originalLine.Data with { twosided = true, blocking = false});
                newLine.TextureInfo = texture;
                return newLine;
            }
            else
            {
                throw new NotImplementedException("this needs more work");
            }
        }).ToArray();
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
