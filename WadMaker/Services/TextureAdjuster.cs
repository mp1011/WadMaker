namespace WadMaker.Services;

public class TextureAdjuster
{
    /// <summary>
    /// Ensures textures are properly aligned
    /// </summary>
    /// <param name="mapElements"></param>
    public void AdjustOffsetsAndPegs(MapElements mapElements)
    {
        int totalWallWidth = 0;
        
        foreach(var line in LineWithNeighbors(mapElements.LineDefs.First(), mapElements))
        {
            var textureSize = DoomTextureConfig.DoomTextureSizes[line.Front.Data.texturemiddle.ToString()];
            line.Front.Data = line.Front.Data with { offsetx = totalWallWidth % textureSize.Width };
            totalWallWidth += (int)line.Length;
        }
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
