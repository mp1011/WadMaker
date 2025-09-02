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
        if (ratio < 2.7)
            return ResourceBalance.Adequate;
        if (ratio < 3.5)
            return ResourceBalance.Comfortable;
        else
            return ResourceBalance.Generous;
    }
}