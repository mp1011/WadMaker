
namespace WadMaker.Tests.Services;

public class ShapeModifierTests
{
    [Test]
    public void CanInvertCorners()
    {
        var modifier = new InvertCorners {  Width = 10 };

        var points = new[]
        {
            new Point(0, 0),
            new Point(100, 0),
            new Point(100, -100),
            new Point(0,-100)
        };

        var modifiedPoints = modifier.AlterPoints(points, new Room());

        var expectedPoints = new[]
        {
            new Point(0, -10),
            new Point(10, -10),
            new Point(10, 0),

            new Point(90, 0),
            new Point(90, -10),
            new Point(100, -10),

            new Point(100, -90),
            new Point(90, -90),
            new Point(90, -100),

            new Point(10, -100),
            new Point(10, -90),
            new Point(0, -90)
        };

        Assert.That(modifiedPoints, Is.EquivalentTo(expectedPoints));
    }
}
