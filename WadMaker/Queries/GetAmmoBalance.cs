namespace WadMaker.Queries;

/// <summary>
/// Calculates the balance of ammo to monsters in the provided rooms.
/// 
/// </summary>
public class GetAmmoBalance
{
    public ResourceBalance Execute(IEnumerable<Room> rooms)
    {
        var monsterHp = new CountMonsterHp().Execute(rooms);
        var meanWeaponDamage = new CountMeanWeaponDamage().Execute(rooms);

        if (monsterHp == 0)
            return ResourceBalance.UnableToCalculate;

        var ratio = meanWeaponDamage / (double)monsterHp;

        if (ratio < 1)
            return ResourceBalance.Insufficient;
        if (ratio < 2)
            return ResourceBalance.BarelyEnough;
        if (ratio >= 4.0)
            return ResourceBalance.Generous;

        return ResourceBalance.Adequate;
    }
}