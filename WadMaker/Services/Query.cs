namespace WadMaker.Services;

public class Query
{
    public CountAmmoAmounts CountAmmoAmounts { get; }
    public CountMeanWeaponDamage CountMeanWeaponDamage { get; }
    public CountMonsterHp CountMonsterHp { get; }
    public GetAmmoBalance GetAmmoBalance { get; }
    public IsPointInSector IsPointInSector { get; }
    public TextureQuery TextureQuery { get; }

    public Query(CountAmmoAmounts countAmmoAmounts, CountMeanWeaponDamage countMeanWeaponDamage, CountMonsterHp countMonsterHp, GetAmmoBalance getAmmoBalance, IsPointInSector isPointInSector, TextureQuery textureQuery)
    {
        CountAmmoAmounts = countAmmoAmounts;
        CountMeanWeaponDamage = countMeanWeaponDamage;
        CountMonsterHp = countMonsterHp;
        GetAmmoBalance = getAmmoBalance;
        IsPointInSector = isPointInSector;
        TextureQuery = textureQuery;
    }
}
