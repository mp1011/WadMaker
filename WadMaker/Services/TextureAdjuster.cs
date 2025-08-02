namespace WadMaker.Services;

public class TextureAdjuster
{
    /// <summary>
    /// Ensures textures are properly aligned
    /// </summary>
    /// <param name="mapElements"></param>
    public void AdjustOffsetsAndPegs(MapElements mapElements)
    {
        foreach(var textureRun in TextureRuns(mapElements))
        {
            AlignTextures(textureRun);
        }
    }

    private void AlignTextures(LineDefPath lines)
    {
        int totalWallWidth = 0;

        foreach (var line in lines)
        {
            var textureSize = DoomTextureConfig.DoomTextureSizes[line.Front.Texture];
            line.Front.Data = line.Front.Data with { offsetx = totalWallWidth % textureSize.Width };
            totalWallWidth += (int)line.Length;
        }
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
        while(--loopProtect >= 0)
        {
            if (loopProtect == 0)
                throw new Exception("Unable to get line paths");

            var usedLines = paths.SelectMany(p => p.ToArray()).ToArray();

            var nextUnusedLine = mapElements.LineDefs.FirstOrDefault(p=> !usedLines.Contains(p));
            if(nextUnusedLine != null)
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
