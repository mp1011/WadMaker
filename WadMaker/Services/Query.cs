namespace WadMaker.Services;

public class Query
{
    public CountAmmoAmounts CountAmmoAmounts { get; }
    public CountMeanWeaponDamage CountMeanWeaponDamage { get; }
    public CountMonsterHp CountMonsterHp { get; }
    public GetAmmoBalance GetAmmoBalance { get; }
    public IsPointInSector IsPointInSector { get; }
    public GetThingSector GetThingSector { get; }
    public IsThingInBounds IsThingInBounds { get; }

    public IsOverlappingAnotherThingOfSameCategory IsOverlappingAnotherThingOfSameCategory { get; }

    public Query(CountAmmoAmounts countAmmoAmounts, CountMeanWeaponDamage countMeanWeaponDamage, CountMonsterHp countMonsterHp, 
        GetAmmoBalance getAmmoBalance, IsPointInSector isPointInSector, GetThingSector getThingSector, IsThingInBounds isThingInBounds,
        IsOverlappingAnotherThingOfSameCategory isOverlappingAnotherThingOfSameCategory)
    {
        CountAmmoAmounts = countAmmoAmounts;
        CountMeanWeaponDamage = countMeanWeaponDamage;
        CountMonsterHp = countMonsterHp;
        GetAmmoBalance = getAmmoBalance;
        IsPointInSector = isPointInSector;
        GetThingSector = getThingSector;    
        IsThingInBounds = isThingInBounds;
        IsOverlappingAnotherThingOfSameCategory = isOverlappingAnotherThingOfSameCategory;
    }
}
