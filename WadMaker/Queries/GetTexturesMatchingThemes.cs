namespace WadMaker.Queries;

public class GetTexturesMatchingThemes
{
    public static Texture[] Execute(string[] ThemeNames, string ColorName)
    {
        var positiveThemes = ThemeNames.Where(p => !p.StartsWith("-")).ToArray();
        var negativeThemes = ThemeNames.Where(p => p.StartsWith("-")).Select(p=> p.TrimStart('-')).ToArray();

        var matches = DoomTextureConfig.DoomTextureInfo.Values
            .Where(p=>p.Themes.ContainsAll(positiveThemes) && !p.Themes.ContainsAny(negativeThemes)
                      && p.Color == ColorName)
            .ToArray();

        return matches.Select(p => Enum.Parse<Texture>(p.Name)).ToArray();
    }
}
