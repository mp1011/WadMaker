using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;

namespace WadMaker.Queries;

public class IsPointInSector
{
    private const int LoopLimit = 100000;

    public bool Execute(Point p, Sector sector, MapElements elements)
    {
        var sectorLines = elements.LineDefs.Where(p => p.BelongsTo(sector));
        if(!sectorLines.Any()) return false;

        var polygons = SectorPolygons(sectorLines).ToArray();
        var polygonsAtPoint = polygons.Where(polygon => IsPointInsidePolygon(p, polygon)).ToArray();

        // if point is in two polygons at once, one of them must be a void inside the sector
        return polygonsAtPoint.Length == 1;
    }

    private bool IsPointInsidePolygon(Point point, Point[] polygon)
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
    private IEnumerable<Point[]> SectorPolygons(IEnumerable<LineDef> lines)
    {
        var lineList = lines.ToList();

        var safety = 0;
        while (lineList.Any())
        {
            var loop = LineLoop(lineList.First(), lineList);
            yield return loop.ToArray();

            lineList.RemoveAll(p => loop.Any(q => p.Contains(q)));

            if (safety++ > LoopLimit)
                throw new Exception("Unable to determine line loops");
        }
    }

    private IEnumerable<Point> LineLoop(LineDef start, IEnumerable<LineDef> lines) 
    {
        var remainingLines = lines.ToList();
        List<Point> polygonPoints = new List<Point>();
        polygonPoints.Add(start.V1);
        polygonPoints.Add(start.V2);

        remainingLines.Remove(start);
        var safety = 0;

        while (true)
        {
            var previousPoint = polygonPoints.Last();

            var v1Match = remainingLines.FirstOrDefault(p => p.V1 == previousPoint);
            var v2Match = remainingLines.FirstOrDefault(p => p.V2 == previousPoint);

            var next = (v1Match ?? v2Match);
            if (next == null)
                throw new Exception("Unclosed sector loop found");
            remainingLines.Remove(next);

            if (v1Match != null)
            {
                if (v1Match.V2 == polygonPoints.First())
                    return polygonPoints;
                polygonPoints.Add(v1Match.V2);
            }
            else if (v2Match != null)
            {
                if (v2Match.V1 == polygonPoints.First())
                    return polygonPoints;
                polygonPoints.Add(v2Match.V1);
            }

            if (safety++ > LoopLimit)
                throw new Exception("Unable to determine line loops");
        }
    }

}
