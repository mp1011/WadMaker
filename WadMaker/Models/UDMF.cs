namespace WadMaker.Models;
public record sector(
    string texturefloor,
    string textureceiling,
    int heightfloor,
    int heightceiling,
    int lightlevel
) : IMapElement;

public record vertex(
    double x,
    double y
) : IMapElement;

public record linedef(
    int v1 = -1,
    int v2 = -1,
    int sidefront = -1,
    int? sideback = null,
    bool? twoSided = null,
    bool blocking = true,
    string? comment = null
) : IMapElement;

public record sidedef(
    int sector,
    string? texturemiddle,
    string? texturetop=null,
    string? texturebottom=null
) : IMapElement;

public record thing(
    double x,
    double y,
    int angle,
    int type,
    bool skill1,
    bool skill2,
    bool skill3,
    bool skill4,
    bool skill5,
    bool single,
    bool dm,
    bool coop
) : IMapElement;