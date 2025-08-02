namespace WadMaker.Models;

public record Theme(ThemeRule[] Rules);

public record ThemeRule(TextureInfo Texture, Func<LineDef, Sector, bool> Condition);
