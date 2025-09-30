namespace WadMaker;

public static class StaticFlags
{
    public static bool ClearUpperAndLowerTexturesOnOneSidedLines;
    public static bool InnerSectorLinesAlwaysStartTwoSided;
    public static bool ClearUnusedMapElements;
    static StaticFlags()
    {
        Reset();
    }

    public static void Reset()
    {
        ClearUpperAndLowerTexturesOnOneSidedLines = true;
        InnerSectorLinesAlwaysStartTwoSided = false;
        ClearUnusedMapElements = true;
    }
}
