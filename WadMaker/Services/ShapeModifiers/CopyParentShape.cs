namespace WadMaker.Services.ShapeModifiers;

public class CopyParentShape : IShapeInitializer
{
    private readonly Padding _padding;
    private readonly IWithShape _parent;

    public bool BoundingBoxFromPoints => true;

    public CopyParentShape(Padding padding, IWithShape parent)
    {
        _padding = padding;
        _parent = parent;
    }

    public Point[] InitializePoints(Shape shape)
    {
        var parentPoints = _parent.Shape.CalculatePoints();
        var center = parentPoints.CentralPoint();

        return parentPoints.Select(p => AlterPoint(p, center))
                           .ToArray();
    }

    private Point AlterPoint(Point src, Point center)
    {
        var angle = center.AngleTo(src);

        if (angle >= 0 && angle < 90)
            return src.Add(-_padding.Right, -_padding.Top);
        else if(angle < 180)
            return src.Add(_padding.Left, -_padding.Top);
        else if(angle < 270)
            return src.Add(_padding.Left, _padding.Bottom);
        else
            return src.Add(-_padding.Right, _padding.Bottom);
    }
}
