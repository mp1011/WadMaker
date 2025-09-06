namespace WadMaker.Queries;

public class GetThingSector
{
    private readonly IsPointInSector _isPointInSector;
    public GetThingSector(IsPointInSector isPointInSector)
    {
        _isPointInSector = isPointInSector;
    }

    public Sector? Execute(Thing thing, MapElements elements) => Execute(thing.Position, elements);

    public Sector? Execute(Point point, MapElements elements)
    {
        return elements.Sectors.FirstOrDefault(sector => _isPointInSector.Execute(point, sector, elements));
    }
}
