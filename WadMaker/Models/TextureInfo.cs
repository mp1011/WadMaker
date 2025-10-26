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
    AutoAlignment? AutoAlign = null,
    TextureInfo? Alternate = null,
    bool IgnoreColumnStops = false)
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
}

public record AutoAlignment(string? RegionLabel, PointF TexturePosition, PointF WallPosition, TexturePart Part)
{
    public Point CalcOffset(string texture, LineDef line)
    {
        var textureInfo = DoomConfig.DoomTextureInfo[texture];
        var region = textureInfo.Regions.Single(p => p.Label == RegionLabel);
        int wallHeight = new WallHeight(line, Part).Execute();
        Point regionCursor = new Point((int)(TexturePosition.X * region.Width), (int)(TexturePosition.Y * region.Height));
        Point wallCursor = new Point((int)(line.Length * WallPosition.X), (int)(wallHeight * WallPosition.Y));

        Point textureCursor = regionCursor.Add(region.X, region.Y);

        if(Part == TexturePart.Upper)
            return new Point(textureCursor.X - wallCursor.X, textureCursor.Y + wallCursor.Y);
        else 
            return new Point(textureCursor.X - wallCursor.X, textureCursor.Y - wallCursor.Y);
    }
}