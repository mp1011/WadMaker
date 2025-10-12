namespace WadMaker.Models;

public record TextureInfo(
    TextureQuery? Main,
    TextureQuery? Upper = null,
    TextureQuery? Mid = null,
    TextureQuery? Lower = null,
    bool? UpperUnpegged = null, // linedef
    bool? LowerUnpegged = null, // linedef
    int? OffsetX = null, //sidedef
    int? OffsetY = null, //sidedef
    bool? DrawLowerFromBottom = null,
    AutoAlignment? AutoAlign = null)
{

    public TextureInfo(Texture? Main = Texture.STONE,
        Texture? Upper = null,
        Texture? Mid = null,
        Texture? Lower = null,
        bool? UpperUnpegged = null, // linedef
        bool? LowerUnpegged = null, // linedef
        int? OffsetX = null, //sidedef
        int? OffsetY = null, //sidedef
        bool? DrawLowerFromBottom = null) : this(
            Main: Main.HasValue ? new TextureQuery(Main.Value) : null,
            Upper: Upper.HasValue ? new TextureQuery(Upper.Value) : null,
            Mid: Mid.HasValue ? new TextureQuery(Mid.Value) : null,
            Lower: Lower.HasValue ? new TextureQuery(Lower.Value) : null,
            UpperUnpegged: UpperUnpegged,
            LowerUnpegged: LowerUnpegged,
            OffsetX: OffsetX,
            OffsetY: OffsetY,
            DrawLowerFromBottom: DrawLowerFromBottom)
    {

    }

    public static TextureInfo Default => new TextureInfo();

    public TextureInfo(LineDef line) : this(
        Main: (line.Front.Data.texturemiddle ?? line.Front.Data.texturetop ?? line.Front.Data.texturebottom).ParseAs<Texture>(),
        Upper: line.Front.Data.texturetop.ParseAs<Texture>(),
        Mid: line.Front.Data.texturemiddle.ParseAs<Texture>(),
        Lower: line.Front.Data.texturebottom.ParseAs<Texture>(),
        UpperUnpegged: line.Data.dontpegtop,
        LowerUnpegged: line.Data.dontpegbottom,
        OffsetX: line.Front.Data.offsetx,
        OffsetY: line.Front.Data.offsety,
        DrawLowerFromBottom: null)
    {
    }


    public override string ToString() => (Main ?? Mid ?? Upper ?? Lower)?.ToString() ?? "";

    public TextureQuery GetQuery(TexturePart part)
    {
        return part switch
        {
            TexturePart.Upper => Upper ?? Main ?? TextureQuery.Missing,
            TexturePart.Lower => Lower ?? Main ?? TextureQuery.Missing,
            _ => Mid ?? Main ?? TextureQuery.Missing,
        };
    }

    public Texture ResolveTexture(TexturePart part, LineDef line)
    {
        return GetQuery(part).Execute(line, part).FirstOrDefault();
    }

    public void ApplyTo(LineDef line)
    {
        line.Data = line.Data with {  dontpegbottom = LowerUnpegged, dontpegtop = UpperUnpegged };
        ApplyTo(line.Front, line, line.Data.twosided);
        ApplyTo(line.Back, line,line.Data.twosided);

        if (DrawLowerFromBottom.GetValueOrDefault())
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
        if(line.Back != null)
            line.Back.Data = line.Back.Data with { offsety = -floorDifference };
    }

    public void ApplyTo(SideDef? side, LineDef line, bool? twosided)
    {
        if (side == null)
            return;

        if(twosided.HasValue && twosided.Value)
        {
            side.Data = side.Data with
            {
                texturemiddle = null,
                texturetop = ResolveTexture(TexturePart.Upper, line).ToString(),
                texturebottom = ResolveTexture(TexturePart.Lower, line).ToString(),
                offsetx = OffsetX,
                offsety = OffsetY
            };
        }
        else if(!Legacy.Flags.HasFlag(LegacyFlags.DontClearUpperAndLowerTexturesOnOneSidedLines))
        {
            var texture = ResolveTexture(TexturePart.Middle, line).ToString();
            var autoOffset = AutoAlign?.CalcOffset(texture, line) ?? new Point(0, 0);
            int ox = autoOffset.X + OffsetX.GetValueOrDefault();
            int oy = autoOffset.Y + OffsetY.GetValueOrDefault();

            side.Data = side.Data with
            {
                texturemiddle = ResolveTexture(TexturePart.Middle, line).ToString(),
                texturebottom = null,
                texturetop = null,
                offsetx = ox == 0 ? null : ox,
                offsety = oy == 0 ? null : oy
            };
        }
        else
        {
            side.Data = side.Data with
            {
                texturemiddle = ResolveTexture(TexturePart.Middle, line).ToString(),
                offsetx = OffsetX,
                offsety = OffsetY
            };
        }
    }
}

public record AutoAlignment(string? RegionLabel, PointF texturePosition, PointF wallPosition)
{
    public Point CalcOffset(string texture, LineDef line)
    {
        // note - currently only works with single sided linse
        if (line.Back != null)
            throw new NotSupportedException("Not supported (yet)");

        var textureInfo = DoomConfig.DoomTextureInfo[texture];
        var region = textureInfo.Regions.Single(p => p.Label == RegionLabel);

        Point regionCursor = new Point((int)(texturePosition.X * region.Width), (int)(texturePosition.Y * region.Height));
        Point wallCursor = new Point((int)(line.Length * wallPosition.X), (int)(line.Front.Sector.Height * wallPosition.Y));

        Point textureCursor = regionCursor.Add(region.X, region.Y);

        return new Point(textureCursor.X - wallCursor.X, textureCursor.Y - wallCursor.Y);
    }
}