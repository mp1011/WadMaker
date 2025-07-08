namespace WadMaker.Models.Basic;

public record Indexed<T>(int Index, T Item);

public static class Indexed
{
    public static Indexed<vertex> Vertex(IDProvider idProvider, int x, int y) =>
       new Indexed<vertex>(idProvider.NextVertex(), new vertex(x, y));

    public static Indexed<sidedef> SideDef(IDProvider idProvider, int sector, string texturemiddle) =>
        new Indexed<sidedef>(idProvider.NextSideDef(), new sidedef(sector, texturemiddle));

    public static Indexed<sector> Sector(IDProvider idProvider, 
        string texturefloor,
        string textureceiling,
        int heightfloor,
        int heightceiling,
        int lightlevel) => new Indexed<sector>(idProvider.NextSectorIndex(),
            new sector(texturefloor, textureceiling, heightfloor, heightceiling, lightlevel));
}
