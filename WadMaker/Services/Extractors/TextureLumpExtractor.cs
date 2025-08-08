namespace WadMaker.Services.Extractors;

static class TextureLumpExtractor
{
  //courtesy of chatgpt
  static DoomTextureSize[] ParseTexture1(byte[] lumpData)
  {
    var textures = new List<DoomTextureSize>();
    using (var ms = new MemoryStream(lumpData))
    using (var reader = new BinaryReader(ms))
    {
      int numTextures = reader.ReadInt32();

      // Read the offset table
      int[] offsets = new int[numTextures];
      for (int i = 0; i < numTextures; i++)
      {
        offsets[i] = reader.ReadInt32();
      }

      for (int i = 0; i < numTextures; i++)
      {
        reader.BaseStream.Seek(offsets[i], SeekOrigin.Begin);

        // Read name (8 bytes, padded with nulls)
        byte[] nameBytes = reader.ReadBytes(8);
        string name = Encoding.ASCII.GetString(nameBytes).TrimEnd('\0');

        reader.ReadInt32(); // masked (unused)
        int width = reader.ReadInt16();
        int height = reader.ReadInt16();

        reader.ReadInt32(); // unused
        int patchCount = reader.ReadInt16();

        // Skip over patch entries (10 bytes each)
        reader.BaseStream.Seek(patchCount * 10, SeekOrigin.Current);

        textures.Add(new DoomTextureSize(name, width, height));
      }
    }

    return textures.ToArray();
  }

  public static DoomTextureSize[] ParseTextures(string path)
  {
    return ParseTexture1(File.ReadAllBytes(path));
  }

  public static string TextureEnumCode(DoomTextureSize[] textures)
  {
    var sb = new StringBuilder();
    string indent = "    ";
    sb.AppendLine("public enum Texture");
    sb.AppendLine("{");
    sb.Append(indent).AppendLine("MISSING,");

    foreach (DoomTextureSize tex in textures)
      sb.Append(indent).Append(tex.Name).AppendLine(",");

    sb.Append(indent).AppendLine("Default = STONE");
    sb.AppendLine("}");
    return sb.ToString();
  }

  public static void ExtractInfoFromTextureLump(string filePath)
  {
    var textures = ParseTextures(filePath);
    var enumString = TextureEnumCode(textures);
    var textureJson = JsonSerializer.Serialize(textures);

    Console.WriteLine("Done");
  }

  //probably shouldn't be here
  public static string ThingEnumCode(IEnumerable<DoomThingInfo> infos)
  {
    var sb = new StringBuilder();
    string indent = "    ";
    sb.AppendLine("public enum ThingType");
    sb.AppendLine("{");
    sb.Append(indent).AppendLine("Unknown = 0,");

    foreach (DoomThingInfo info in infos)
    {
      var desc = info.Description.Replace(' ', '_');
      desc = Regex.Replace(desc, @"[^a-zA-Z0-9_]", "");
      sb.Append(indent).Append(desc).Append(" = ").Append(info.Decimal).AppendLine(",");
    }

    sb.AppendLine("}");
    return sb.ToString();
  }
}
