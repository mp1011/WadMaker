namespace WadMaker;

public static class StaticFlags
{
    public static bool ClearUpperAndLowerTexturesOnTwoSidedLines;

    static StaticFlags()
    {
        Reset();
    }

    public static void Reset()
    {
        ClearUpperAndLowerTexturesOnTwoSidedLines = true;
    }
}
