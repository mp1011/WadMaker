namespace WadMaker.Extensions;

public static class EnumExtensions
{
    public static ValueRange MonsterCount(this EnemyDensity density) => 
        density switch
        {
            EnemyDensity.Single => new ValueRange(1, 1),
            EnemyDensity.Rare => new ValueRange(1, 2),
            EnemyDensity.Sparse => new ValueRange(1, 3),
            EnemyDensity.Common => new ValueRange(3, 5),
            EnemyDensity.Excessive => new ValueRange(4, 8),
            _ => throw new ArgumentOutOfRangeException()
        };

}
