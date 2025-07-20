namespace WadMaker.Models;

public record Door(int Thickness, TextureInfo Texture, TextureInfo TrackTexture, int PositionInHall)
{
    public Door(int Thickness, Texture Texture, Texture TrackTexture, int PositionInHall) :
        this(Thickness, new TextureInfo(Main: Texture), new TextureInfo(Main: TrackTexture, LowerUnpegged: true), PositionInHall)
    { }
}
