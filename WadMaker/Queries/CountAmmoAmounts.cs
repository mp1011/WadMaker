namespace WadMaker.Queries;

class CountAmmoAmounts
{
  public Dictionary<ThingType, int> Execute(IEnumerable<Room> rooms)
  {
    Dictionary<ThingType, int> ammoAmounts = new();
    return rooms.SelectMany(r => r.Things)
      .SelectMany(r => r.ThingType.AmmoWeaponAmounts())
      .GroupBy(k => k.Item1)
      .ToDictionary(k => k.Key, v => v.Sum(x=>x.Item2)); 
  }
}