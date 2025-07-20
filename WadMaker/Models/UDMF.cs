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
    bool? twosided = null,
    bool blocking = true,
    string? comment = null,
    int? special = null,
    int? arg0 = null,
    int? arg1 = null,
    int? arg2 = null,
    int? arg3 = null,
    int? arg4 = null,
    bool? playeruse = null,
    bool? repeatspecial = null,
    bool? dontpegtop = null,
    bool? dontpegbottom = null
) : IMapElement;

public record sidedef(
    int sector,
    string? texturemiddle,
    string? texturetop=null,
    string? texturebottom=null,
    int? offsetx=null,
    int? offsety=null,
    string? comment=null
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