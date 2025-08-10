namespace WadMaker.Services;

public class ThingPlacer
{
    public void AddThing(ThingType thing, Room room,
      double pctX,
      double pctY,
      ThingFlags flags = ThingFlags.AllSkills | ThingFlags.Single,
      Angle? angle = null)
    {
        AddThing(thing,
            room,
            (int)(room.UpperLeft.X + room.Bounds.Width * pctX),
            (int)(room.UpperLeft.Y - room.Bounds.Height * pctY),
            flags,
            angle);
    }

    public void AddThing(ThingType thing, Room room,
      int posX,
      int posY,
      ThingFlags flags = ThingFlags.AllSkills | ThingFlags.Single,
      Angle? angle = null)
    {
        room.Things.Add(new Thing(new thing(
               x: posX,
               y: posY,
               angle: angle ?? Angle.East,
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

    public void AddFormation(ThingType type, Room room, int count, Angle angle, ThingFlags flags, double centerX, double centerY, int spacing)
    {
        if (count == 0)
            return;
        if (count == 1)
        {
            AddThing(type, room, centerX, centerY, flags, angle);
            return;
        }

        int halfLength = ((count - 1) * spacing) / 2;

        var center = new Point((int)(room.UpperLeft.X + room.Bounds.Width * centerX),
                               (int)(room.UpperLeft.Y - room.Bounds.Height * centerY));

        var start = angle.RotateClockwise(90).ToPoint(halfLength).Add(center);
        var end = angle.RotateCounterClockwise(90).ToPoint(halfLength).Add(center);

        for (int i = 0; i < count; i++)
        {
            var position = start.MoveToward(end, spacing * i);
            AddThing(type, room, position.X, position.Y, flags, angle);
        }
    }

    public void AddPlayerStartToFirstRoomCenter(Map map)
    {
        AddThing(ThingType.Player1_start, map.Rooms.First(), 0.5, 0.5);
    }
}