namespace WadMaker.Services.ShapeModifiers;


/// <summary>
/// Turns the sector into a regular polygon with n sides
/// </summary>
public class NGon : IShapeModifier
{
    public int Sides { get; set; } = 16;

    public Point[] AlterPoints(Point[] points)
    {
        if (points.Length != 4)
            return points;

        int width, height;
        if (points[0].X == points[1].X)
        {
            height = (int)points[0].DistanceTo(points[1]);
            width = (int)points[1].DistanceTo(points[2]);
        }
        else
        {
            width = (int)points[0].DistanceTo(points[1]);
            height = (int)points[1].DistanceTo(points[2]);
        }

        var center = points.CentralPoint();

        List<Point> vertices = new List<Point>();

        // Center of the bounding box
        float cx = center.X;
        float cy = center.Y;

        // Radius must fit in the bounding box (circle inscribed in the rectangle)
        float rx = width / 2f;
        float ry = height / 2f;
        float radius = Math.Min(rx, ry);  // Keep polygon fully inside the box

        // Angle between each point (in radians)
        float angleStep = 2f * (float)Math.PI / Sides;

        // Optional: rotate polygon to make it "upright" (e.g., first point on top)
        float angleOffset = -MathF.PI / 2;

        for (int i = 0; i < Sides; i++)
        {
            float angle = i * angleStep + angleOffset;
            float x = cx + radius * MathF.Cos(angle);
            float y = cy + radius * MathF.Sin(angle);
            vertices.Add(new Point((int)x, (int)y));
        }

        // algorithm puts vertices in the wrong order
        vertices.Reverse();

        return vertices.ToArray();
    }
}

