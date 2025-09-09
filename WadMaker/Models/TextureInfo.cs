namespace WadMaker.Models;

public record TextureInfo(
    Texture? Main = Texture.STONE,
    Texture? Upper = null,
    Texture? Mid = null,
    Texture? Lower = null,
    bool? UpperUnpegged = null, // linedef
    bool? LowerUnpegged = null, // linedef
    int? OffsetX = null, //sidedef`
    int? OffsetY = null) //sidedef
{
    public TextureInfo(LineDef line) : this(
        Main: (line.Front.Data.texturemiddle ?? line.Front.Data.texturetop ?? line.Front.Data.texturebottom).ParseAs<Texture>(),
        Upper: line.Front.Data.texturetop.ParseAs<Texture>(),
        Mid: line.Front.Data.texturemiddle.ParseAs<Texture>(),
        Lower: line.Front.Data.texturebottom.ParseAs<Texture>(),
        UpperUnpegged: line.Data.dontpegtop,
        LowerUnpegged: line.Data.dontpegbottom,
        OffsetX: line.Front.Data.offsetx,
        OffsetY: line.Front.Data.offsety)
    {
    }

    public string UpperString() => Upper?.ToString() ?? this.ToString();

    public string LowerString() => Lower?.ToString() ?? this.ToString();    

    public override string ToString()
    {
        return (Main ?? Mid ?? Upper ?? Lower ?? Texture.MISSING).ToString();
    }

    public void ApplyTo(LineDef line)
    {
        line.Data = line.Data with {  dontpegbottom = LowerUnpegged, dontpegtop = UpperUnpegged };
        ApplyTo(line.Front, line.Data.twosided);
        ApplyTo(line.Back, line.Data.twosided);
    }

    public void ApplyTo(SideDef? side, bool? twosided)
    {
        if (side == null)
            return;

        if(twosided.HasValue && twosided.Value)
        {
            side.Data = side.Data with
            {
                texturemiddle = null,
                texturetop = (Upper ?? Main ?? Texture.MISSING).ToString(),
                texturebottom = (Lower ?? Main ?? Texture.MISSING).ToString(),
                offsetx = OffsetX,
                offsety = OffsetY
            };
        }
        else if(StaticFlags.ClearUpperAndLowerTexturesOnTwoSidedLines)
        {
            side.Data = side.Data with
            {
                texturemiddle = (Mid ?? Main ?? Texture.MISSING).ToString(),
                texturebottom = null,
                texturetop = null,
                offsetx = OffsetX,
                offsety = OffsetY
            };
        }
        else
        {
            side.Data = side.Data with
            {
                texturemiddle = (Mid ?? Main ?? Texture.MISSING).ToString(),
                offsetx = OffsetX,
                offsety = OffsetY
            };
        }
    }
}
