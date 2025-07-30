namespace WadMaker.Tests.Services;

internal class TextureAdjusterTests : StandardTest
{
    [Test]
    public void CanAlignTextures()
    {
        var map = new Map();
        map.Rooms.Add(new Room
        {
            UpperLeft = Point.Empty,
            BottomRight = new Point(70, -100),
            WallTexture = new TextureInfo(Texture.BRICK7),
        });

        var elements = MapBuilder.Build(map);
        TextureAdjuster.AdjustOffsetsAndPegs(elements);

        Assert.That(elements.LineDefs[0].Front.Data.offsetx, Is.EqualTo(0));
        Assert.That(elements.LineDefs[1].Front.Data.offsetx, Is.EqualTo(6));
        Assert.That(elements.LineDefs[2].Front.Data.offsetx, Is.EqualTo(42));
        Assert.That(elements.LineDefs[3].Front.Data.offsetx, Is.EqualTo(48));

    }
}
