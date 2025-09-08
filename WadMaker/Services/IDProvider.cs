namespace WadMaker.Services;
public class IDProvider
{
    private int _nextVertex = 0;
    public int NextVertex()
    {
        var ret = _nextVertex;
        _nextVertex++;
        return ret;
    }

    private int _nextSidedef= 0;
    public int NextSideDef()
    {
        var ret = _nextSidedef;
        _nextSidedef++;
        return ret;
    }

    private int _nextSectorIndex = 1;
    public int NextSectorIndex()
    {
        var ret = _nextSectorIndex;
        _nextSectorIndex++;
        return ret;
    }
}

