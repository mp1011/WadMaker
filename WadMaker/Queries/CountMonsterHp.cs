namespace WadMaker.Queries;

/// <summary>
/// Gets the total hitpoints of all monsters in the provided rooms.
/// </summary>
public class CountMonsterHp
{
    public int Execute(IEnumerable<Room> rooms)
    {
        return rooms.SelectMany(r => r.Things)
                    .Select(t => t.ThingType.GetHp())
                    .Sum();
    }
}