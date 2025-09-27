namespace WadMaker.Models;

public record Door(int Thickness, TextureInfo Texture, TextureInfo TrackTexture, int PositionInHall, int? Tag = null, 
    KeyType KeyColor = KeyType.None, DoorColorBar? ColorBar = null)
{
    public Door(int Thickness, Texture Texture, Texture TrackTexture, int PositionInHall, int? Tag = null) :
        this(Thickness, new TextureInfo(Main: Texture), new TextureInfo(Main: TrackTexture, LowerUnpegged: true), PositionInHall, Tag)
    { }

    public LineSpecial DoorSpecial()
    {
        if (KeyColor == KeyType.None)
            return new DoorRaise(0, Speed.StandardDoor);
        else
            return new DoorLockedRaise(0, Speed.StandardDoor, Lock: KeyColor);
    }
}
