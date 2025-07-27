namespace WadMaker.Models;

public class Cutout : IShape
{
    public List<IShapeModifier> ShapeModifiers { get; } = new List<IShapeModifier>();

    public Point UpperLeft { get; set; } = Point.Empty;
    public Point BottomRight { get; set; } = Point.Empty;
    public Texture WallTexture { get; set; } = Texture.Default;

    public Cutout Copy()
    {
        var copy = new Cutout
        {
            UpperLeft = UpperLeft,
            BottomRight = BottomRight,
            WallTexture = WallTexture
        };
        copy.ShapeModifiers.AddRange(ShapeModifiers);
        return copy;
    }

    public Cutout RelativeTo(IShape parent)
    {
         var copy = new Cutout
         {
             UpperLeft = parent.UpperLeft.Add(UpperLeft),
             BottomRight = parent.UpperLeft.Add(BottomRight),
             WallTexture = WallTexture,             
         };
        copy.ShapeModifiers.AddRange(ShapeModifiers);
        return copy;
    }
}
