namespace WadMaker.Services.ShapeModifiers;

public interface IShapeModifier
{
    Point[] AlterPoints(Point[] points, Room room);
}
