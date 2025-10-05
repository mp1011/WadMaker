namespace WadMaker.Models;

public interface IShape
{
    Point UpperLeft { get; set; }
    Point BottomRight { get; set; }
    Point Center { get; set; }

    List<IShapeModifier> ShapeModifiers { get; }

    bool Owns(IShape other);
}
