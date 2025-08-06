namespace WadMaker.Config;

public static class DoomConfig
{
  private static Dictionary<string, DoomTextureSize> _sizes;
  private static Dictionary<string, DoomTextureInfo> _infos;
  private static Dictionary<int, DoomThingInfo> _things;

  public static Dictionary<string, DoomTextureSize> DoomTextureSizes => _sizes ??= ParseDoomTextureSizes();
  public static Dictionary<string, DoomTextureInfo> DoomTextureInfo => _infos ??= ParseDoomTextureInfo();
  public static Dictionary<int, DoomThingInfo> DoomThingInfo => _things ??= ParseDoomThingInfo();

  public static string[] AllThemes() => DoomTextureInfo
      .SelectMany(p => p.Value.Themes)
      .Order()
      .Distinct()
      .ToArray();

  public static string[] AllColors() => DoomTextureInfo
     .Select(p => p.Value.Color)
     .Where(p => p != null)
     .Order()
     .Distinct()
     .ToArray();

  private static string ReadFile(string name) => File.ReadAllText(name);

  private static JsonSerializerOptions JsonSerializerOptions { get; } = new JsonSerializerOptions
  {
    AllowTrailingCommas = true
  };

  private static Dictionary<string, DoomTextureSize> ParseDoomTextureSizes()
  {
    try
    {
      var sizes = JsonSerializer.Deserialize<DoomTextureSize[]>(ReadFile("config/texture_sizes.json"), JsonSerializerOptions);
      return sizes.ToDictionary(k => k.Name, v => v);
    }
    catch (Exception e)
    {
      throw;
    }
  }

  private static Dictionary<string, DoomTextureInfo> ParseDoomTextureInfo()
  {
    var infos = JsonSerializer.Deserialize<DoomTextureInfo[]>(ReadFile("config/texture_info.json"), JsonSerializerOptions);

    infos = infos!.Select(i => i with { Size = DoomTextureSizes.GetValueOrDefault(i.Name) }).ToArray();

    return infos.ToDictionary(k => k.Name, v => v);
  }

  private static Dictionary<int, DoomThingInfo> ParseDoomThingInfo()
  {
    using (var reader = new StreamReader("config/thing_info.csv"))
    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
    {
      var records = csv.GetRecords<DoomThingInfo>();
      return records.ToDictionary(k => k.Decimal, v => v);
    }
  }

}
