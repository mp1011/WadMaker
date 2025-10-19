namespace WadMaker;

[Flags]
public enum LegacyFlags
{
    None = 0,
    InnerSectorLinesAlwaysStartTwoSided = 2,
    DontClearUnusedMapElements = 4,
    DisableMoveTowardRoundingFix = 8,
    OverwriteExistingXOffset = 16,
    IgnoreColumnStops = 32
}
public static class Legacy
{
    public static LegacyFlags Flags = LegacyFlags.None;
}
