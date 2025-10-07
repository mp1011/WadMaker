namespace WadMaker.Models.Placement;

public class AnchorPoint
{
    private IWithShape _shape;

    private Point _absolutePosition;

    public AnchorPoint(IWithShape shape, Point relativePoint)
    {
        _absolutePosition = shape.UpperLeft.Add(relativePoint.X, relativePoint.Y); 
        _shape = shape;
    }

    public Point Position
    {
        get => _absolutePosition;
        set
        {
            var delta = new Point(_absolutePosition.X - _shape.UpperLeft.X,
                                  _absolutePosition.Y - _shape.UpperLeft.Y);

            var size = _shape.Bounds().Size;

            _shape.UpperLeft = new Point(value.X - delta.X, value.Y - delta.Y); 
            _shape.BottomRight = _shape.UpperLeft.Add(size.Width, -size.Height);
            _absolutePosition = value;
        }
    }

    public override string ToString() => _absolutePosition.ToString();
}
