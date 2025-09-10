namespace WadMaker.Models;

public record ValueRange(int Min, int Max, Random Randomizer)
{
    public int RandomValue() => Randomizer.Next(Min, Max + 1);
}
