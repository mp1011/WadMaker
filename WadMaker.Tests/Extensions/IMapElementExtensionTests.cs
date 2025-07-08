namespace WadMaker.Tests.Extensions;

public class IMapElementExtensionTests
{
    [Test]
    public void Paint_Sector_ProducesExpectedString()
    {
        var sector = new sector(
            texturefloor: "FLAT1",
            textureceiling: "FLAT1",
            heightfloor: 0,
            heightceiling: 128,
            lightlevel: 192
        );

        var sb = new StringBuilder();
        sector.Paint(sb);

        // Assert
        var expected = """
        sector
        {
            texturefloor = "FLAT1";
            textureceiling = "FLAT1";
            heightfloor = 0;
            heightceiling = 128;
            lightlevel = 192;
        }

        """;

        Assert.That(sb.ToString(), Is.EqualTo(expected));
    }
}
