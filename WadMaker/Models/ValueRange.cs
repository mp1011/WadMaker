namespace WadMaker.Models;

public record ValueRange(int Min, int Max)
{
    public int RandomValue() => Random.Shared.Next(Min, Max + 1);
}
