namespace WadMaker.Services;

public class ThingPlacer
{
    private readonly OverlappingThingResolver _overlappingThingResolver;

    public ThingPlacer(OverlappingThingResolver overlappingThingResolver)
    {
        _overlappingThingResolver = overlappingThingResolver;
    }

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
        var newThing = new Thing(new thing(
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
           ));

        // is it overkill to do this every time?
        _overlappingThingResolver.Execute(newThing, room.Things);

        room.Things.Add(newThing);
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

    public void AddMonsters(PlayerPath path, params MonsterPlacement[] monsterPlacements)
    {
        foreach(var placement in monsterPlacements)
        {
            foreach(var node in placement.NodeRange(path))
            {
                AddMonsters(node, placement);
            }
        }
    }

    private void AddMonsters(PlayerPathNode node, MonsterPlacement placement)
    {
        if(placement.Density == EnemyDensity.None)
            return;

        ValueRange countPerRoom = placement.Density switch
        {
            EnemyDensity.Single => new ValueRange(1, 1),
            EnemyDensity.Rare => new ValueRange(1, 2),
            EnemyDensity.Sparse => new ValueRange(1, 3),
            EnemyDensity.Common => new ValueRange(3, 5),
            EnemyDensity.Excessive => new ValueRange(4, 8),
            _ => throw new ArgumentOutOfRangeException()
        };

        var chancePerRoom = placement.Density switch
        {
            EnemyDensity.Single => 0.25,
            EnemyDensity.Rare => 0.25,
            EnemyDensity.Sparse => 0.5,
            EnemyDensity.Common => 0.75,
            EnemyDensity.Excessive => 0.90,
            _ => throw new ArgumentOutOfRangeException()
        };

        int maxPasses = 20;
        int monstersPlaced = 0;
        while (monstersPlaced == 0 && --maxPasses > 0)
        {
            foreach (var room in node.RequiredRooms)
            {
                bool placeMonsters = Random.Shared.NextDouble() <= chancePerRoom;
                if (!placeMonsters)
                    continue;

                int countForRoom = countPerRoom.RandomValue();
                if (countForRoom == 0)
                    continue;

                AddFormation(placement.Monster, room, countForRoom, Angle.East, ThingFlags.AllSkillsAndModes, 0.5, 0.5, 32);
                monstersPlaced += countForRoom;
            }
        }
    }
}