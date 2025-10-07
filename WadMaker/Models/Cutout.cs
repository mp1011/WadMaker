namespace WadMaker.Models;

public class Cutout : IWithShape
{
    public Point UpperLeft
    {
        get => Shape.UpperLeft;
        set => Shape.UpperLeft = value;
    }

    public Point BottomRight
    {
        get => Shape.BottomRight;
        set => Shape.BottomRight = value;
    }

    public Point Center
    {
        get => Shape.Center;
        set => Shape.Center = value;
    }
    public Texture WallTexture { get; set; } = Texture.STONE;

    public Shape Shape { get; private set; }

    public Cutout(Point? upperLeft = null, Point? center = null, Size ? size = null)
    {
        Shape = new Shape(upperLeft, center, size);
    }

    public Cutout Copy()
    {
        var copy = new Cutout
        {
            WallTexture = WallTexture
        };
        copy.Shape = Shape.Copy();
        return copy;
    }

    public Cutout RelativeTo(IWithShape parent)
    {
         var copy = new Cutout
         {
             UpperLeft = parent.UpperLeft.Add(UpperLeft),
             BottomRight = parent.UpperLeft.Add(BottomRight),
             WallTexture = WallTexture,             
         };
        copy.Shape.Modifiers.AddRange(Shape.Modifiers);
        return copy;
    }
}
