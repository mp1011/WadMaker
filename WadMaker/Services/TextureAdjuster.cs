using static System.Net.Mime.MediaTypeNames;

namespace WadMaker.Services;

public class TextureAdjuster
{
    private readonly IConfig _config;

    public TextureAdjuster(IConfig config)
    {
        _config = config;
    }

    /// <summary>
    /// Ensures textures are properly aligned
    /// </summary>
    /// <param name="mapElements"></param>
    public MapElements AdjustOffsetsAndPegs(MapElements mapElements)
    {
        foreach (var textureRun in TextureRuns(mapElements))
        {
            AlignTextures(textureRun);
        }

        SetLinePegs(mapElements);
        return mapElements;
    }
    
    public MapElements ApplyThemes(MapElements mapElements)
    {
        foreach (var sector in mapElements.Sectors)
        {
            if (sector.Room.Theme == null)
                continue;

            foreach (var line in mapElements.LineDefs.Where(p => p.BelongsTo(sector)))
            {
                ApplyTheme(sector.Room.Theme, sector, line);
            }
        }

        return mapElements;
    }

    private void ApplyTheme(Theme theme, Sector sector, LineDef line)
    {
        var matchingRule = theme.Rules.FirstOrDefault(p => p.AppliesTo(line));
        if (matchingRule == null)
            return;

        matchingRule.GetTexture(line).ApplyTo(line);
    }

    private void SetLinePegs(MapElements mapElements)
    {
        foreach (var twoSidedLine in mapElements.LineDefs.Where(p => p.Back != null))
        {
            if (twoSidedLine.LineSpecial != null)
                continue;
            
            if(twoSidedLine.Data.dontpegbottom == null)
                twoSidedLine.Data = twoSidedLine.Data with { dontpegbottom = true };

            if (twoSidedLine.Data.dontpegtop == null)
                twoSidedLine.Data = twoSidedLine.Data with { dontpegtop = true };
        }
    }

    private void AlignTextures(LineDefPath lines)
    {
        int totalWallWidth = 0;
        LineDef? prev = null;

        foreach (var line in lines)
        {
            line.Front.Data = line.Front.Data with
            {
                offsetx = CalcXOffset(line, totalWallWidth),
                offsety = prev != null ? CalcYOffset(line, prev) : line.Front.Data.offsety
            };
            totalWallWidth += (int)line.Length;
            prev = line;
        }
    }

    private int CalcXOffset(LineDef line, int totalWallWidth)
    {
        if (line.Front.Data.offsetx.HasValue && !Legacy.Flags.HasFlag(LegacyFlags.OverwriteExistingXOffset))
        {
            return line.Front.Data.offsetx.Value;
        }

        var textureSize = DoomConfig.DoomTextureSizes[line.Front.Texture];
        return totalWallWidth % textureSize.Width;
    }

    private int? CalcYOffset(LineDef line, LineDef previousLine)
    {
        if (!_config.HandleYOffet)
            return null;

        if (line.Front.Data.offsety.HasValue)
            return line.Front.Data.offsety;

        var prevFloor = previousLine.Front.Sector.Data.heightfloor;
        var thisFloor = line.Front.Sector.Data.heightfloor;
        var prevCeiling = previousLine.Front.Sector.Data.heightceiling;
        var thisCeiling = line.Front.Sector.Data.heightceiling;

        var ceilingDiff = prevCeiling - thisCeiling;
        var offsetY = previousLine.Front.Data.offsety.GetValueOrDefault() + ceilingDiff;
        if (offsetY == 0)
            return null;
        else
            return offsetY;

        // todo, how to handle floor?
    }

    /// <summary>
    /// Gets all sequences of linedefs that share the same texture
    /// </summary>
    /// <param name="mapElements"></param>
    /// <returns></returns>
    private IEnumerable<LineDefPath> TextureRuns(MapElements mapElements)
    {
        var linePaths = GetLinePaths(mapElements).ToArray();
        return linePaths.SelectMany(p => TextureRuns(p));
    }

    private IEnumerable<LineDefPath> GetLinePaths(MapElements mapElements)
    {
        var line = mapElements.LineDefs.First();
        var path = new LineDefPath(mapElements, line);

        var paths = path.Build().ToList();

        int loopProtect = 1000000;
        while (--loopProtect >= 0)
        {
            if (loopProtect == 0)
                throw new Exception("Unable to get line paths");

            var usedLines = paths.SelectMany(p => p.ToArray()).ToArray();

            var nextUnusedLine = mapElements.LineDefs.FirstOrDefault(p => !usedLines.Contains(p));
            if (nextUnusedLine != null)
            {
                var newPaths = new LineDefPath(mapElements, nextUnusedLine).Build().ToArray();
                paths.AddRange(newPaths);
            }
            else
            {
                break;
            }
        }

        return paths;
    }



    private IEnumerable<LineDefPath> TextureRuns(LineDefPath run)
    {
        return run.SplitBy(LinesShareTexture);
    }

    private bool LinesShareTexture(LineDef line1, LineDef line2)
    {
        if (line1.Front.Texture == line2.Front.Texture)
            return true;

        if (line2.Back?.Texture != null && line1.Front.Texture == line2.Back.Texture)
            return true;

        if (line2.Back?.Texture != null && line1.Back?.Texture == line2.Back.Texture)
            return true;

        if (line1.Back?.Texture != null && line1.Back?.Texture == line2.Front.Texture)
            return true;

        return false;
    }


}
