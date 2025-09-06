namespace WadMaker.Queries;

public class IsOverlappingAnotherThingOfSameCategory
{
    public bool Execute(Thing thing, MapElements elements)
    {
        var thingsInSameCategory = elements.Things.Where(p => p.Category == thing.Category && p != thing);
        if(!thingsInSameCategory.Any())
            return false;

        return thingsInSameCategory.Any(p=> thing.Overlaps(p));
    }
}
