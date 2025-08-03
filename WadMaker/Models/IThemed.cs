namespace WadMaker.Models;

public interface IThemed
{
    Theme? Theme { get; }   
}

public class NoTheme : IThemed
{
    public static NoTheme Instance { get; } = new NoTheme();
    public Theme? Theme => null;
}