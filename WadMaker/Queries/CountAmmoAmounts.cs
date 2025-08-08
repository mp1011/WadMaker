namespace WadMaker.Queries;

class CountAmmoAmounts
{
  public Dictionary<ThingType, int> Execute(IEnumerable<Room> rooms)
  {
    Dictionary<ThingType, int> ammoAmounts = new();
    return rooms.SelectMany(r => r.Things)
      .SelectMany(r => r.ThingType.AmmoWeaponAmounts())
      .ToDictionary(k => k.Item1, v => v.Item2); 
  }
}