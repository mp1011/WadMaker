
namespace WadMaker.Services.ShapeModifiers;

public class CopyParentShape : IShapeModifier
{
    private readonly Padding _padding;
    private readonly IShape _parent;

    public CopyParentShape(Padding padding, IShape parent)
    {
        _padding = padding;
        _parent = parent;
    }

    public Point[] AlterPoints(Point[] points, IShape room)
    {
        var parentPoints = _parent.GetPoints();
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
