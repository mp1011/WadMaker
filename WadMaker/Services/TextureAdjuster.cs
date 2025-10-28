namespace WadMaker.Services;

public class TextureAdjuster
{
    private readonly IConfig _config;
    private readonly Random _random;

    public TextureAdjuster(IConfig config, Random random)
    {
        _config = config;
        _random = random;
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
    
    public MapElements ApplyTextures(MapElements mapElements)
    {
        foreach (var line in mapElements.LineDefs)
        {
            ApplyTexture(line);
        }
        return mapElements;
    }

    public MapElements ApplyThemes(MapElements mapElements)
    {
        foreach (var sector in mapElements.Sectors)
        {
            if (sector.Room.Theme == null)
                continue;

            ApplyTheme(sector);

            foreach (var line in mapElements.LineDefs.Where(p => p.BelongsTo(sector)))
            {
                ApplyTheme(sector.Room.Theme, sector, line);
            }
        }

        return mapElements;
    }

    private void ApplyTheme(Sector sector)
    {
        var theme = sector.Room.Theme;
        if (theme == null)
            return;

        var matchingRule = theme.SectorRules.FirstOrDefault(p => p.AppliesTo(sector));
        if (matchingRule == null)
            return;

        sector.Floor = matchingRule.GetFloor(sector);
        sector.Ceiling = matchingRule.GetCeiling(sector);
    }

    private void ApplyTheme(Theme theme, Sector sector, LineDef line)
    {
        var matchingRule = theme.LineRules.FirstOrDefault(p => p.AppliesTo(line));
        if (matchingRule == null)
            return;

        foreach(var side in line.SideDefs.Where(p=>p.Sector == sector))
            side.TextureInfo = GetTextureFromThemeRule(line, matchingRule);

        ApplyTexture(line);
    }

    private TextureInfo GetTextureFromThemeRule(LineDef lineDef, ThemeRule rule)
    {
        if (rule.Query == null)
            return rule.Texture ?? TextureInfo.Default;

        var upper = GetTextureFromQuery(lineDef, rule.Query, TexturePart.Upper);
        var middle = GetTextureFromQuery(lineDef, rule.Query, TexturePart.Middle);
        var lower = GetTextureFromQuery(lineDef, rule.Query, TexturePart.Lower);

        if (rule.Texture == null)
        {
            if(lineDef.SingleSided)
                return new TextureInfo(Mid: middle);
            else 
                return new TextureInfo(Upper: upper, Lower: lower);
        }

        if (lineDef.SingleSided)
        {
            return rule.Texture with
            {
                Mid = rule.Texture.Mid ?? new TextureQuery(middle),
            };
        }
        else
        {
            return rule.Texture with
            {
                Upper = rule.Texture.Upper ?? new TextureQuery(upper),
                Mid = rule.Texture.Mid,
                Lower = rule.Texture.Lower ?? new TextureQuery(lower)
            };
        }
    }

    private Texture GetTextureFromQuery(LineDef lineDef, TextureQuery query, TexturePart texturePart)
    {
        var matches = query.Execute(lineDef, texturePart);
        switch(query.Distribution)
        {
            case TextureDistribution.FirstMatch:
                return matches.FirstOrDefault();
            case TextureDistribution.Random:
                return matches.PickRandom(_random);
            default:
                return Texture.MISSING;
        }      
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
            int offsetX = CalcXOffset(line, totalWallWidth);
            if(!line.TextureInfo.IgnoreColumnStops && !OffsetAlignsWithColumnStops(line,offsetX))
            {
                line.TextureInfo = line.TextureInfo.Alternate ?? throw new Exception("Texture does not fit to wall and no alternate was provided");

                //not sure about this
                line.Front.TextureInfo = null;
                
                offsetX = 0;
                ApplyTexture(line);
                totalWallWidth = -(int)line.Length;
            }

            line.Front.Data = line.Front.Data with
            {
                offsetx = offsetX,
                offsety = prev != null ? CalcYOffset(line, prev) : line.Front.Data.offsety
            };
            totalWallWidth += (int)line.Length;
            prev = line;
        }
    }

    private bool OffsetAlignsWithColumnStops(LineDef line, int offsetX)
    {
        if(Legacy.Flags.HasFlag(LegacyFlags.IgnoreColumnStops))
            return true;

        var texture = TextureForAlignment(line);
        var textureInfo = DoomConfig.DoomTextureInfo[texture.ToString()];
        if(textureInfo.ColumnStops == null)
            return true;

        int leftSide = offsetX % textureInfo.Size!.Width;
        int rightSide = (offsetX + (int)line.Length) % textureInfo.Size!.Width;

        return textureInfo.ColumnStops.Contains(leftSide) && textureInfo.ColumnStops.Contains(rightSide);   
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

            var usedLines = paths.SelectMany(p => p.ToArray()).Distinct().ToArray();

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

    public Texture ResolveTextureOrDefault(TexturePart part, LineDef line, SideDef side)
    {
        return ResolveTexture(part, line, side) ?? Texture.MISSING;
    }

    public Texture? ResolveTexture(TexturePart part, LineDef line, SideDef side)
    {
        var textureInfo = side.TextureInfo ?? line.TextureInfo;
        if (textureInfo == null)
            return null;

        return textureInfo.GetQuery(part, line.Back != null)?.Execute(line, part)?.FirstOrDefault();
    }

    public void ApplyTexture(LineDef line)
    {
        var lineTexture = TextureForLine(line);
        line.Data = line.Data with { dontpegbottom = lineTexture.LowerUnpegged, dontpegtop = lineTexture.UpperUnpegged };
        
        ApplyTexture(line.Front, line, line.Data.twosided);
        ApplyTexture(line.Back, line, line.Data.twosided);

        if(lineTexture.DrawLowerFromBottom.GetValueOrDefault())
            Apply_DrawLowerFromBottom(line);
    }

    /// <summary>
    /// Sets the Y Offset to the negative of the floor difference
    /// </summary>
    /// <param name="line"></param>
    private void Apply_DrawLowerFromBottom(LineDef line)
    {
        line.Data = line.Data with { dontpegbottom = false };

        var floors = line.Sectors.Select(p => p.FloorHeight).ToArray();
        var floorDifference = floors.Max() - floors.Min();

        line.Front.Data = line.Front.Data with { offsety = -floorDifference };
        if (line.Back != null)
            line.Back.Data = line.Back.Data with { offsety = -floorDifference };
    }

    /// <summary>
    /// Determines the best texture to use for the line itself
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    private TextureInfo TextureForLine(LineDef line)
    {
        if (line.SingleSided)
            return line.Front.TextureInfo ?? line.TextureInfo;

        if(line.Front.Sector.FloorHeight > line.Back!.Sector.FloorHeight)
            return line.Front.TextureInfo ?? line.TextureInfo;

        if (line.Front.Sector.CeilingHeight > line.Back!.Sector.CeilingHeight)
            return line.Front.TextureInfo ?? line.TextureInfo;

        if (line.Back!.Sector.FloorHeight > line.Front.Sector.FloorHeight)
            return line.Back.TextureInfo ?? line.TextureInfo;

        if (line.Back!.Sector.CeilingHeight > line.Front.Sector.CeilingHeight)
            return line.Back.TextureInfo ?? line.TextureInfo;

        return line.TextureInfo;
    }

    private void ApplyTexture(SideDef? side, LineDef line, bool? twosided)
    {
        if (side == null || line.TextureInfo == null)
            return;

        if (twosided.HasValue && twosided.Value)
        {
            side.Data = side.Data with
            {
                texturemiddle = ResolveTexture(TexturePart.Middle, line, side)?.ToString(),
                texturetop = ResolveTextureOrDefault(TexturePart.Upper, line, side).ToString(),
                texturebottom = ResolveTextureOrDefault(TexturePart.Lower, line, side).ToString(),
                offsetx = line.TextureInfo.OffsetX,
                offsety = line.TextureInfo.OffsetY
            };
        }
        else
        {
            side.Data = side.Data with
            {
                texturemiddle = ResolveTextureOrDefault(TexturePart.Middle, line, side).ToString(),
                texturebottom = null,
                texturetop = null,
            };
        }


        if (line.TextureInfo.AutoAlign != null)
        {
            line.Data = line.Data with { dontpegtop = false, dontpegbottom = false };
            var autoOffset = line.TextureInfo.AutoAlign?.CalcOffset(side.Texture, line) ?? new Point(0, 0);
            int ox = autoOffset.X + line.TextureInfo.OffsetX.GetValueOrDefault();
            int oy = autoOffset.Y + line.TextureInfo.OffsetY.GetValueOrDefault();

            side.Data = side.Data with
            {
                offsetx = ox == 0 ? null : ox,
                offsety = oy == 0 ? null : oy
            };

        }
    }

    /// <summary>
    /// Returns the texture used to determine alignment.
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    private Texture TextureForAlignment(LineDef line)
    {
        // needs improvement
        if (line.SingleSided)
            return line.Front.Data.texturemiddle.ParseAs<Texture>() ?? Texture.MISSING;
        else
            return line.Front.Data.texturebottom.ParseAs<Texture>() ?? Texture.MISSING;
    }
}
