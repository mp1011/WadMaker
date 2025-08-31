namespace WadMaker.Services;

public class OverlappingThingResolver
{
    /// <summary>
    /// Ensures there are no overlaps when the new Thing is added to the room
    /// </summary>
    /// <param name="newThing">New Thing which might overlap another Thing</param>
    /// <param name="things">Existing Things, assumed to be already non-overlapping</param>
    public void Execute(Thing newThing, IEnumerable<Thing> things)
    {       
        var thingList = things.ToList();
        int maxTries = 1000;

        // check for overlap with things as big or bigger
        while (--maxTries > 0)
        {
            var nextOverlap = thingList.FirstOrDefault(p => p.Overlaps(newThing) && p.Radius >= newThing.Radius);
            if (nextOverlap == null)
                break;

            SeparateThings(newThing, nextOverlap);
        }

        // check for overlap with smaller things
        maxTries = 1000;
        while (--maxTries > 0)
        {
            var nextOverlap = thingList.FirstOrDefault(p => p.Overlaps(newThing) && p.Radius < newThing.Radius);
            if(nextOverlap == null)
                break;

            var list = things.ToList();
            list.Add(newThing);
            list.Remove(nextOverlap);
            Execute(nextOverlap, list);
        }
    }


    private void SeparateThings(Thing mover, Thing anchor)
    {
        int maxSteps = 100;

        // continue moving object forward by 4 pixels until no longer overlapping
        while (--maxSteps > 0 && mover.Overlaps(anchor))
        {
            mover.Position = mover.Position.Move(mover.Data.angle, 4);
        }
    }
}

