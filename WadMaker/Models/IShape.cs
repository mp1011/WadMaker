namespace WadMaker.Models;

public interface IShape
{
    Point UpperLeft { get; set; }
    Point BottomRight { get; set; }

    List<IShapeModifier> ShapeModifiers { get; }
}
