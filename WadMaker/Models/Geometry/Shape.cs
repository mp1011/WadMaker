namespace WadMaker.Models.Geometry;

public interface IWithShape
{
    Point UpperLeft { get; set; }
    Point BottomRight { get; set; }
    Point Center { get; set; }
    Shape Shape { get; }
}

public class Shape
{
    public Shape(Point? upperLeft = null, Point? center = null, Size? size = null)
    {
        size ??= new Size(128, 128);

        if (upperLeft != null)
        {
            UpperLeft = upperLeft.Value;
            BottomRight = new Point(UpperLeft.X + size.Value.Width, UpperLeft.Y - size.Value.Height);
        }
        else
        {
            center ??= Point.Empty;
            UpperLeft = new Point(center.Value.X - size.Value.Width / 2, center.Value.Y + size.Value.Height / 2);
            BottomRight = new Point(center.Value.X + size.Value.Width / 2, center.Value.Y - size.Value.Height / 2);
        }           
    }

    public Shape? RelativeTo { get; set; }

    public List<IShapeModifier> Modifiers { get; } = new List<IShapeModifier>();

    public IShapeInitializer Initializer { get; set; } = new BoundingBoxInitializer();

    private Point _upperLeft = Point.Empty, _bottomRight = Point.Empty;

    public Point UpperLeft
    {
        get
        {
            if(Initializer.BoundingBoxFromPoints)
            {
                var points = CalculatePoints();
                _upperLeft = new Point(points.Min(p => p.X), points.Max(p => p.Y));
                if(RelativeTo != null)
                    _upperLeft = _upperLeft.Add(-RelativeTo.UpperLeft.X, -RelativeTo.UpperLeft.Y);
            }

            return _upperLeft;
        }
        set => _upperLeft = value;
    }
    public Point BottomRight
    {
        get
        {
            if (Initializer.BoundingBoxFromPoints)
            {
                var points = CalculatePoints();
                _bottomRight = new Point(points.Max(p => p.X), points.Min(p => p.Y));
                if (RelativeTo != null)
                    _bottomRight = _bottomRight.Add(-RelativeTo.UpperLeft.X, -RelativeTo.UpperLeft.Y);
            }

            return _bottomRight;
        }
        set => _bottomRight = value;
    }

    public Point Center
    {
        get => Bounds.Center;
        set
        {
            Point delta = new Point(value.X - Bounds.Center.X, value.Y - Bounds.Center.Y);
            UpperLeft = UpperLeft.Add(delta);
            BottomRight = BottomRight.Add(delta);
        }
    }

    /// <summary>
    /// Note, center point is preserved on size change
    /// </summary>
    public Size Size
    {
        get => Bounds.Size;
        set
        {
            var center = Center;
            UpperLeft = new Point(center.X - value.Width / 2, center.Y + value.Height / 2);
            BottomRight = new Point(center.X + value.Width / 2, center.Y - value.Height / 2);
        }
    }

    public Point[] CalculatePoints()
    {
        var initialPoints = Initializer.InitializePoints(this, _upperLeft, _bottomRight);

        return Modifiers
            .Aggregate(initialPoints, (p, s) => s.AlterPoints(p))
            .ToArray();
    }

    public DRectangle Bounds
    {
        get => new DRectangle(
         location: UpperLeft,
         size: new Size(BottomRight.X - UpperLeft.X,
             Math.Abs(BottomRight.Y - UpperLeft.Y)));
    }

    public void SetFromVertices(IEnumerable<Point> points)
    {
        UpperLeft = new Point(points.Min(p => p.X), points.Max(p => p.Y));
        BottomRight = new Point(points.Max(p => p.X), points.Min(p => p.Y));
    }

    /// <summary>
    /// Interprets the coordinates as absolute and adjusts them to be relative to the current parent
    /// </summary>
    public void AdjustToRelative()
    {
        if (RelativeTo == null)
            return;

        var size = Size;

        UpperLeft = UpperLeft.Add(-RelativeTo.UpperLeft.X, -RelativeTo.UpperLeft.Y);
        BottomRight = new Point(UpperLeft.X + size.Width, UpperLeft.Y - size.Height);
    }

    public Shape Copy()
    {
        var copy = new Shape(center: Center, size: Bounds.Size);
        copy.Initializer = Initializer;
        copy.Modifiers.AddRange(Modifiers);
        return copy;
    }
}
