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

    private void AlignTextures(LineDef[] lines)
    {
        int totalWallWidth = 0;

        foreach (var line in lines)
        {
            var textureSize = DoomTextureConfig.DoomTextureSizes[line.Front.Data.texturemiddle];
            line.Front.Data = line.Front.Data with { offsetx = totalWallWidth % textureSize.Width };
            totalWallWidth += (int)line.Length;
        }
    }

    /// <summary>
    /// Gets all sequences of linedefs that share the same texture
    /// </summary>
    /// <param name="mapElements"></param>
    /// <returns></returns>
    private IEnumerable<LineDef[]> TextureRuns(MapElements mapElements)
    {
        var linesWithNeighbors = mapElements.LineDefs
            .Select(p => LineWithNeighbors(p, mapElements).ToArray())
            .GroupBy(p => String.Join(",", p.Select(q => mapElements.LineDefs.IndexOf(q)).Order().ToArray()))
            .Select(p => p.First())
            .ToArray();

        return linesWithNeighbors.SelectMany(p => TextureRuns(p));
    }

    private IEnumerable<LineDef[]> TextureRuns(LineDef[] run)
    {
        var lastTexture = run[0].Front.Data.texturemiddle;
        List<LineDef[]> runs = new List<LineDef[]>();
        List<LineDef> currentRun = new List<LineDef>();

        foreach(var line in run)
        {
            if (line.Front.Data.texturemiddle != lastTexture)
            {
                runs.Add(currentRun.ToArray());
                currentRun.Clear();
                lastTexture = line.Front.Data.texturemiddle;
            }
            
            currentRun.Add(line);            
        }

        if (runs.Count() > 0 && lastTexture == run.First().Front.Data.texturemiddle)
        {
            currentRun.AddRange(runs.First());
            runs[0] = currentRun.ToArray();
        }
        else
        {

            runs.Add(currentRun.ToArray());
        }

        return runs;
    }


    private IEnumerable<LineDef> LineWithNeighbors(LineDef line, MapElements mapElements)
    {
        yield return line;
        var next = mapElements.LineDefs.FirstOrDefault(p => p.V1 == line.V2);
        while(next != null && next != line)
        {
            yield return next;
            next = mapElements.LineDefs.FirstOrDefault(p => p.V1 == next.V2);            
        }
    }
}
