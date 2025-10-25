namespace WadMaker.Queries;

public class IsPointInSector
{
    private const int LoopLimit = 100000;

    public bool Execute(Point p, Sector sector, MapElements elements)
    {
        if (!sector.Room.Bounds.ContainsPoint(p))
            return false;

        var polygons = sector.PolygonsCache;
        if (polygons == null)
        {
            var sectorLines = elements.LineDefs.Where(p => p.BelongsTo(sector) && p.Length > 0);
            if (!sectorLines.Any()) return false;

            polygons = SectorPolygons(sectorLines).ToArray();
            sector.PolygonsCache = polygons;
        }
        var polygonsAtPoint = polygons.Where(polygon => IsPointInsidePolygon(p, polygon)).ToArray();

        // if point is in two polygons at once, one of them must be a void inside the sector
        return polygonsAtPoint.Length == 1;
    }

    private bool IsPointInsidePolygon(Point point, PointPath polygon)
    {
        bool isInside = false;
        int j = polygon.Length - 1;

        for (int i = 0; i < polygon.Length; i++)
        {
            PointF pi = polygon[i];
            PointF pj = polygon[j];

            bool intersect = ((pi.Y > point.Y) != (pj.Y > point.Y)) &&
                             (point.X < (pj.X - pi.X) * (point.Y - pi.Y) / (pj.Y - pi.Y + float.Epsilon) + pi.X);

            if (intersect)
                isInside = !isInside;

            j = i;
        }

        return isInside;
    }
    private IEnumerable<PointPath> SectorPolygons(IEnumerable<LineDef> lines)
    {
        List<PointPath> paths = new List<PointPath>();

        while (true)
        {
            var unusedPoint = lines.SelectMany(p=>p.Vertices)
                .FirstOrDefault(p=> !paths.Any(path => path.Contains(p)));

            if (unusedPoint == null)
                break;

            paths.AddRange(GetPathsFromPoint(unusedPoint, lines));
        }

        return paths.Where(p => p.IsLooping);
    }

    private PointPath[] GetPathsFromPoint(Point point, IEnumerable<LineDef> lines)
    {
        if(!lines.Any())
            return Array.Empty<PointPath>();

        List<PointPath> paths = new List<PointPath>();
        LineDef nextLine = lines.First(p => p.Contains(point));       
        PointPath currentPath = new PointPath(nextLine.V1, nextLine.V2);       
        paths.Add(currentPath);
        int currentPathIndex = 0;

        while(currentPathIndex < paths.Count)
        {
            var possibleNextLines = lines.Where(p => p != nextLine && p.Contains(currentPath.Last())).ToArray();
            if(!possibleNextLines.Any())
                currentPathIndex++;
            else if(possibleNextLines.Length == 1)
            {
                nextLine = possibleNextLines.First();
                currentPath.Add(nextLine.OtherVertex(currentPath.Last()));
            }
            else 
            {
                nextLine = MostClockwise(currentPath[^2], currentPath[^1], possibleNextLines);
                
                foreach(var otherLines in possibleNextLines.Except(new[] { nextLine }))
                {
                    var newPath = new PointPath(currentPath.Last());
                    newPath.Add(otherLines.OtherVertex(currentPath.Last()));

                    if(!paths.Any(p=> p[0] == newPath[0] && p[1] == newPath[1]))
                        paths.Add(newPath);
                }

                currentPath.Add(nextLine.OtherVertex(currentPath.Last()));
            }

            if(currentPath.IsLooping)
                currentPathIndex++;
        }

        return paths.ToArray();
    }

    private LineDef MostClockwise(Point beforeAnchor, Point anchor, IEnumerable<LineDef> lineDefs)
    {
        var compareAngle = beforeAnchor.AngleTo(anchor);

        return lineDefs.Select(line =>
        {
            var other = line.OtherVertex(anchor);
            var angle = anchor.AngleTo(other);
            return (line, angle);
        }).OrderBy(p => (p.angle - compareAngle).AsAngle())
        .First().line;

    } 
}
