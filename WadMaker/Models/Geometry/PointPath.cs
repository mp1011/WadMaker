using System.Collections;

namespace WadMaker.Models.Geometry;

public class PointPath : IEnumerable<Point>
{
    public Point? LoopStart { get; private set; }

    public bool IsLooping => LoopStart != null;

    private List<Point> _list = new List<Point>();
    private HashSet<Point> _pointsHash = new HashSet<Point>();

    public int Length => _list.Count;

    public Point this[int index] => _list[index];

    public PointPath(params Point[] initialPoints)
    {
        foreach (var pt in initialPoints)
            Add(pt);
    }

    public bool Contains(Point point) => _pointsHash.Contains(point);

    public void Add(Point point)
    {
        if (Contains(point))
        {
            LoopStart = point;
            return;
        }

        _list.Add(point);
        _pointsHash.Add(point);
    }

    public PointPath Copy()
    {
        var copy = new PointPath();
        copy._list.AddRange(_list);

        foreach (var pt in _list)
            copy._pointsHash.Add(pt);

        copy.LoopStart = LoopStart;
        return copy;
    }

    public IEnumerator<Point> GetEnumerator()
    {
        return ((IEnumerable<Point>)_list).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_list).GetEnumerator();
    }
}
