namespace WadMaker.Queries;

/// <summary>
/// Mean damage that the player can inflict with the ammo found in the provided rooms.
/// </summary>
class CountMeanWeaponDamage
{
    public int Execute(IEnumerable<Room> rooms)
    {
        var ammoAmounts = new CountAmmoAmounts().Execute(rooms);
        return ammoAmounts.Sum(p => p.Key.GetMeanWeaponDamage() * p.Value);
    }
}