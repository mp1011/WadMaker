using System.ComponentModel;

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

    public Thing AddThing(ThingType thing, Room room,
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
        _overlappingThingResolver.Execute(newThing, room.Things.Where(p=>p.ThingType.Category() == newThing.ThingType.Category()));

        room.Things.Add(newThing);
        return newThing;
    }

    public void AddFormation(ThingType type, Room room, int count, Angle angle, ThingFlags flags, ThingPlacement placement, ThingPattern pattern, int spacing)
    {
        var centerX = placement.XPercent;
        var centerY = placement.YPercent;
        if (count == 0)
            return;
        else if (count == 1)
        {
            AddThing(type, room, centerX, centerY, flags, angle);
            return;
        }
        else if (count == 2)
            pattern = ThingPattern.Row; 

        switch(pattern)
        {
            case ThingPattern.Row:
                AddFormationRow(type, room, count, angle, flags, placement, spacing);
                break;
            case ThingPattern.Square:
                AddFormationSquare(type, room, count, angle, flags, placement, spacing);
                return;
            case ThingPattern.Triangle:
                AddFormationTriangle(type, room, count, angle, flags, placement, spacing);
                return;
            case ThingPattern.Circle:
                AddFormationCircle(type, room, count, angle, flags, placement, spacing);
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(pattern), pattern, null);
        }
    }

    private void AddFormationRow(ThingType type, Room room, int count, Angle angle, ThingFlags flags, ThingPlacement placement, int spacing)
    {
        var centerX = placement.XPercent;
        var centerY = placement.YPercent;
        var center = new Point((int)(room.UpperLeft.X + room.Bounds.Width * centerX),
                               (int)(room.UpperLeft.Y - room.Bounds.Height * centerY));

        AddFormationRow(type, room, count, angle, flags, center, spacing);
    }

    private void AddFormationRow(ThingType type, Room room, int count, Angle angle, ThingFlags flags, Point center, int spacing)
    {
        int halfLength = ((count - 1) * spacing) / 2;
        var start = angle.RotateClockwise(90).ToPoint(halfLength).Add(center);
        var end = angle.RotateCounterClockwise(90).ToPoint(halfLength).Add(center);

        for (int i = 0; i < count; i++)
        {
            var position = start.MoveToward(end, spacing * i);
            AddThing(type, room, position.X, position.Y, flags, angle);
        }
    }

    private void AddFormationTriangle(ThingType type, Room room, int count, Angle angle, ThingFlags flags, ThingPlacement placement, int spacing)
    {
        var centerX = placement.XPercent;
        var centerY = placement.YPercent;
        var center = new Point((int)(room.UpperLeft.X + room.Bounds.Width * centerX),
                               (int)(room.UpperLeft.Y - room.Bounds.Height * centerY));

        // find the smallest triangle number which can fit all the elements
        int triangleNumber = 0;
        int i = 0;
        while(triangleNumber < count)
        {
            triangleNumber = i * (i + 1) / 2;
            i++;
        }

        int triangleSection = 1;
        int thingsAdded = 0;

        while(thingsAdded < count)
        {
            int numToAdd = triangleSection;
            if(thingsAdded + numToAdd > count)
                numToAdd = count - thingsAdded;
            AddFormationRow(type, room, numToAdd, angle, flags, center.Move(angle.Opposite, spacing * (triangleSection-1)), (spacing * triangleSection) / numToAdd );

            thingsAdded += numToAdd;
            triangleSection++;
        }
    }

    private void AddFormationSquare(ThingType type, Room room, int count, Angle angle, ThingFlags flags, ThingPlacement placement, int spacing)
    {
        var centerX = placement.XPercent;
        var centerY = placement.YPercent;
        var center = new Point((int)(room.UpperLeft.X + room.Bounds.Width * centerX),
                               (int)(room.UpperLeft.Y - room.Bounds.Height * centerY));

        // find the smallest square number which can fit all the elements
        int squareNumber = 0;
        int i = 0;
        while (squareNumber < count)
        {
            squareNumber = i * i;
            i++;
        }

        int column = 0;
        int thingsAdded = 0;
        int itemsPerCol = (int)Math.Sqrt(squareNumber);

        while (thingsAdded < count)
        {
            int numToAdd = itemsPerCol;
            if (thingsAdded + numToAdd > count)
                numToAdd = count - thingsAdded;
            AddFormationRow(type, room, numToAdd, angle, flags, center.Move(angle.Opposite, spacing * column), spacing);

            thingsAdded += numToAdd;
            column++;
        }
    }

    private void AddFormationCircle(ThingType type, Room room, int count, Angle angle, ThingFlags flags, ThingPlacement placement, int spacing)
    {
        var centerX = placement.XPercent;
        var centerY = placement.YPercent;
        var center = new Point((int)(room.UpperLeft.X + room.Bounds.Width * centerX),
                               (int)(room.UpperLeft.Y - room.Bounds.Height * centerY));


        int anglePerItem = 360 / count;
        var radius = (spacing * count) / (2 * Math.PI); 

        for(int i = 0; i < count; i++)
        {
            var placementAngle = new Angle(anglePerItem * i);

            var position = center.Move(placementAngle.Degrees, (int)radius);
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

    public void AddAmmo(PlayerPath path, ResourceBalance balance)
    {
        foreach(var node in path.Nodes.WithIndex())
        {
            var subset = path.Nodes.Take(node.Index + 1).ToArray();

            var playerWeapons = subset.SelectMany(p => p.RequiredRooms)
                                  .SelectMany(p => p.Things)
                                  .Where(p => p.Category == ThingCategory.Weapon)
                                  .Select(p => p.ThingType)
                                  .Distinct()
                                  .ToArray();

            AddAmmo(subset, playerWeapons, balance);
        }
    }
    
    private void AddAmmo(IEnumerable<PlayerPathNode> nodes, IEnumerable<ThingType> playerWeapons, ResourceBalance balance)
    {
        if (!playerWeapons.Any())
            return;

        var allRooms = nodes.SelectMany(p => p.RequiredRooms).ToArray();

        if (new GetAmmoBalance().Execute(allRooms) == ResourceBalance.UnableToCalculate)
            return;

        int maxTries = 1000;
        while(--maxTries > 0 && new GetAmmoBalance().Execute(allRooms) < balance)
        {
            var weapon = playerWeapons.PickRandom();
            var ammo = weapon.SmallAmmoType();

            var room = allRooms.PickRandom();
            AddThing(ammo, room, 0.5, 0.5, ThingFlags.AllSkillsAndModes);
        }
    }

    private void AddMonsters(PlayerPathNode node, MonsterPlacement placement)
    {
        if(placement.Density == EnemyDensity.None)
            return;

        ValueRange countPerRoom = placement.Density.MonsterCount();

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

                AddFormation(placement.Monster, room, countForRoom, placement.Angle, ThingFlags.AllSkillsAndModes, ThingPlacement.Center, placement.Pattern, 32);
                monstersPlaced += countForRoom;
            }
        }
    }
}