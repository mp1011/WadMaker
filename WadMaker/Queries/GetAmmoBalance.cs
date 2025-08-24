namespace WadMaker.Queries;

/// <summary>
/// Calculates the balance of ammo to monsters in the provided rooms.
/// 
/// </summary>
public class GetAmmoBalance
{
    public AmmoBalance Execute(IEnumerable<Room> rooms)
    {
        var monsterHp = new CountMonsterHp().Execute(rooms);
        var meanWeaponDamage = new CountMeanWeaponDamage().Execute(rooms);

        if (monsterHp == 0)
            return AmmoBalance.UnableToCalculate;

        var ratio = meanWeaponDamage / (double)monsterHp;

        if (ratio < 1)
            return AmmoBalance.Insufficient;
        if (ratio < 2)
            return AmmoBalance.BarelyEnough;
        if (ratio >= 4.0)
            return AmmoBalance.Generous;

        return AmmoBalance.Adequate;
    }
}