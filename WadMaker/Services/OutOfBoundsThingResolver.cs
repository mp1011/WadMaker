using WadMaker.Models;
using WadMaker.Models.Geometry;

namespace WadMaker.Services;

public class OutOfBoundsThingResolver
{
    private readonly Query _query;

    public OutOfBoundsThingResolver(Query query)
    {
        _query = query;
    }

    public void EnsureThingsAreInBounds(Map map, MapElements mapElements)
    {
        foreach(var room in map.Rooms)
        {
            EnsureThingsAreInsideRoom(room, mapElements);
        }
    }

    private void EnsureThingsAreInsideRoom(Room room, MapElements mapElements)
    {
        if(!room.Things.Any())
            return;

        var sectors = mapElements.Sectors.Where(p => p.Room == room).ToArray();
        if(!sectors.Any())
            return;

        var thingsOutOfBounds = room.Things.Where(p => !_query.IsThingInBounds.Execute(p, mapElements)).ToArray();
        if (!thingsOutOfBounds.Any())
            return;

        // try moving all things together
        if (TryMoveThingGroup(thingsOutOfBounds, mapElements))
            return;

        foreach(var thing in thingsOutOfBounds)
        {
            MoveThing(thing, mapElements);
        }
    }

    /// <summary>
    /// Tries to move all things together until they are all in a sector
    /// </summary>
    /// <param name="things"></param>
    /// <param name="mapElements"></param>
    /// <returns></returns>
    private bool TryMoveThingGroup(Thing[] things, MapElements mapElements)
    {
        var angle = things.First().Angle;
        var originalPositions = things.Select(thing => (thing, thing.Position)).ToArray();

        var stepOffset = angle.ToPoint(8);

        for(int distance = 8; distance < 1000; distance += 8)
        {
            foreach(var thing in things)
            {
                thing.Position = new Point(thing.Position.X + stepOffset.X, thing.Position.Y + stepOffset.Y);
            }

            if (things.All(p => _query.IsThingInBounds.Execute(p, mapElements)))
                return true;
        }

        foreach(var originalPos in originalPositions)
        {
            originalPos.thing.Position = originalPos.Position;
        }
        return false;
    }

    private void MoveThing(Thing thing, MapElements mapElements)
    {
        var stepOffset = thing.Angle.ToPoint(8);
        Point originalPosition = thing.Position;

        for (int distance = 8; distance < 1000; distance += 8)
        {               
            thing.Position = new Point(thing.Position.X + stepOffset.X, thing.Position.Y + stepOffset.Y);

            if (_query.IsThingInBounds.Execute(thing, mapElements) &&
                !_query.IsOverlappingAnotherThingOfSameCategory.Execute(thing, mapElements))
                return;
        }

        thing.Position = originalPosition;
        throw new Exception($"Unable to move thing into a sector" ); //maybe fix later?
    }
}
