namespace WadMaker.Services.ShapeModifiers;

public interface IShapeInitializer
{
    Point[] InitializePoints(Shape shape, Point initialUpperLeft, Point initialBottomRight);
    bool BoundingBoxFromPoints => false;
}

public class BoundingBoxInitializer : IShapeInitializer
{
    public Point[] InitializePoints(Shape shape, Point upperLeft, Point bottomRight) =>
        new[]
         {
            new Point(upperLeft.X, upperLeft.Y),
            new Point(bottomRight.X, upperLeft.Y),
            new Point(bottomRight.X, bottomRight.Y),
            new Point(upperLeft.X, bottomRight.Y),
        };    
}
