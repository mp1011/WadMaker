namespace WadMaker.Models.Geometry;

public struct Angle
{
    private double _degrees;

    public double Radians => _degrees * Math.PI / 180.0;

    public double Degrees => _degrees;

    public Angle(double degrees)
    {
        _degrees = degrees.NMod(360.0);
    }

    public Angle Opposite => new Angle(_degrees + 180);

    public Point ToPoint(double length)
    {
        var x = length * Math.Cos(Radians);
        var y = length * Math.Sin(Radians);

        return new Point((int)x, (int)y);
    }

    public Angle RotateClockwise(double degrees)
    {
        return new Angle(_degrees - degrees);
    }

    public Angle RotateCounterClockwise(double degrees)
    {
        return new Angle(_degrees + degrees);
    }

    public static Angle West => new Angle(180.0);
    public static Angle East => new Angle(0.0);
    public static Angle North => new Angle(90.0);
    public static Angle South => new Angle(270.0);

    public static implicit operator double(Angle angle) => angle._degrees;
    public static implicit operator int(Angle angle) => (int)angle._degrees;

}
