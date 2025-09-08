namespace WadMaker.Models;

public record Door(int Thickness, TextureInfo Texture, TextureInfo TrackTexture, int PositionInHall, int? Tag = null)
{
    public Door(int Thickness, Texture Texture, Texture TrackTexture, int PositionInHall, int? Tag = null) :
        this(Thickness, new TextureInfo(Main: Texture), new TextureInfo(Main: TrackTexture, LowerUnpegged: true), PositionInHall, Tag)
    { }
}
