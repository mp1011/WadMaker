namespace WadMaker.Services.ShapeModifiers;

public interface IShapeInitializer
{
    Point[] InitializePoints(Shape shape);
    bool BoundingBoxFromPoints => false;
}

public class BoundingBoxInitializer : IShapeInitializer
{
    public Point[] InitializePoints(Shape shape) =>
        new[]
         {
            new Point(shape.UpperLeft.X, shape.UpperLeft.Y),
            new Point(shape.BottomRight.X, shape.UpperLeft.Y),
            new Point(shape.BottomRight.X, shape.BottomRight.Y),
            new Point(shape.UpperLeft.X, shape.BottomRight.Y),
        };    
}
