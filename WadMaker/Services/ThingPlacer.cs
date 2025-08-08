namespace WadMaker.Services;

public class ThingPlacer
{

  public void AddThing(ThingType thing, Room room,
    double pctX,
    double pctY,
    ThingFlags flags = ThingFlags.AllSkills | ThingFlags.Single,
    int angle = 0)
  {
    room.Things.Add(new Thing(new thing(
           x: room.UpperLeft.X + room.Bounds.Width * pctX,
           y: room.UpperLeft.Y - room.Bounds.Height * pctY,
           angle: angle,
           type: (int)thing,
           skill1: flags.HasFlag(ThingFlags.Skill1),
           skill2: flags.HasFlag(ThingFlags.Skill2),
           skill3: flags.HasFlag(ThingFlags.Skill3),
           skill4: flags.HasFlag(ThingFlags.Skill4),
           skill5: flags.HasFlag(ThingFlags.Skill5),
           single: flags.HasFlag(ThingFlags.Single),
           ambush: flags.HasFlag(ThingFlags.Ambush) ? true : null,
           dm: flags.HasFlag(ThingFlags.Deathmatch),
           coop: flags.HasFlag(ThingFlags.CoOp)
       )));
  }

  public void AddPlayerStartToFirstRoomCenter(Map map)
  {
    AddThing(ThingType.Player1Start, map.Rooms.First(), 0.5, 0.5);
  }
}