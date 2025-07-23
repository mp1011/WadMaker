namespace WadMaker.Tests.Extensions;

class GeometryExtensionTests
{
    [TestCase(0.0, 100, 0)]
    [TestCase(90.0, 0, 100)]
    [TestCase(180.0, -100, 0)]
    [TestCase(270.0, 0, -100)]
    public void AngleToPoint(double angle, int expectedX, int expectedY)
    {
        var pt = angle.AngleToPoint(100);
        Assert.That(pt.X, Is.EqualTo(expectedX));
        Assert.That(pt.Y, Is.EqualTo(expectedY));
    }
}
