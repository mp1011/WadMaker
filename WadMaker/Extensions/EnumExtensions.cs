namespace WadMaker.Extensions;

public static class EnumExtensions
{
    public static ValueRange MonsterCount(this EnemyDensity density, Random random) => 
        density switch
        {
            EnemyDensity.Single => new ValueRange(1, 1, random),
            EnemyDensity.Rare => new ValueRange(1, 2, random),
            EnemyDensity.Sparse => new ValueRange(1, 3, random),
            EnemyDensity.Common => new ValueRange(3, 5, random),
            EnemyDensity.Excessive => new ValueRange(4, 8, random),
            _ => throw new ArgumentOutOfRangeException()
        };

    public static T To<T>(this Enum enumVal) where T : Enum
    {
        if (Enum.TryParse(typeof(T), enumVal.ToString(), out var result))
            return (T)result;
        throw new ArgumentException($"Could not convert {enumVal} to {typeof(T)}");
    }

}
