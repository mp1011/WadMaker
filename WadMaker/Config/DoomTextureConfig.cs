﻿namespace WadMaker.Config;

static class DoomTextureConfig
{    
    private static Dictionary<string, DoomTextureSize> _sizes;
    private static Dictionary<string, DoomTextureInfo> _infos;

    public static Dictionary<string, DoomTextureSize> DoomTextureSizes => _sizes ??= ParseDoomTextureSizes();
    public static Dictionary<string, DoomTextureInfo> DoomTextureInfo => _infos ??= ParseDoomTextureInfo();

    private static string ReadFile(string name) => File.ReadAllText(name);

    private static JsonSerializerOptions JsonSerializerOptions { get; } = new JsonSerializerOptions
    {
        AllowTrailingCommas = true
    };

    private static Dictionary<string, DoomTextureSize> ParseDoomTextureSizes()
    {
        try
        {
            var sizes = JsonSerializer.Deserialize<DoomTextureSize[]>(ReadFile("config\\texture_sizes.json"), JsonSerializerOptions);
            return sizes.ToDictionary(k => k.Name, v => v);
        }
        catch(Exception e)
        {
            throw;
        }
    }

    private static Dictionary<string, DoomTextureInfo> ParseDoomTextureInfo()
    {
        var infos = JsonSerializer.Deserialize<DoomTextureInfo[]>(ReadFile("config\\texture_info.json"), JsonSerializerOptions);
        return infos.ToDictionary(k => k.Name, v => v);
    }

}
