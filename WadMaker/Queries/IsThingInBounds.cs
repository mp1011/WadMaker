namespace WadMaker.Queries;

public class IsThingInBounds
{
    private readonly GetThingSector _getThingSector;

    public IsThingInBounds(GetThingSector getThingSector)
    {
        _getThingSector = getThingSector;
    }

    public bool Execute(Thing thing, MapElements elements)
    {
        if (_getThingSector.Execute(thing, elements) == null)
            return false;

        if (_getThingSector.Execute(thing.Position.Add(-thing.Radius, 0), elements) == null)
            return false;

        if (_getThingSector.Execute(thing.Position.Add(thing.Radius, 0), elements) == null)
            return false;

        if (_getThingSector.Execute(thing.Position.Add(0, -thing.Radius), elements) == null)
            return false;

        if (_getThingSector.Execute(thing.Position.Add(0, thing.Radius), elements) == null)
            return false;

        return true;
    }
}
