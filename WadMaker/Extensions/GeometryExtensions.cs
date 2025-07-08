namespace WadMaker.Extensions;

public static class GeometryExtensions
{
    public static bool Intersects(this vertex v, LineDef line)
    {
        var colinear = (line.V2.x - line.V1.x) * (v.y - line.V1.y) == (line.V2.y - line.V1.y) * (v.x - line.V1.x);
        if (!colinear)
            return false;

        // Bounds check
        bool withinX = v.x >= Math.Min(line.V1.x, line.V2.x) && v.x <= Math.Max(line.V1.x, line.V2.x);
        bool withinY = v.y >= Math.Min(line.V1.y, line.V2.y) && v.y <= Math.Max(line.V1.y, line.V2.y);

        return withinX && withinY;
    }

    public static double SquaredDistanceTo(this vertex v, vertex other)
    {
        double dx = v.x - other.x;
        double dy = v.y - other.y;
        return dx * dx + dy * dy;
    }
}
