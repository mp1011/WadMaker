using NUnit.Framework;
using System.Drawing;
using WadMaker.Config;
using WadMaker.Extensions;
using WadMaker.Models;
using WadMaker.Models.BuildingBlocks;
using WadMaker.Models.Geometry;
using WadMaker.Models.LineSpecials;
using WadMaker.Models.Placement;
using WadMaker.Queries;
using WadMaker.Tests.TestHelpers;

namespace WadMaker.Tests.Performance;

internal class AllTexturesTestMap : StandardTest
{
    public void TextureInfoTestMap(string? theme)
    {
        Legacy.Flags = LegacyFlags.DontResolveCrossingLines;
        var map = new Map();

        var themes = DoomConfig.DoomTextureInfo.Values.SelectMany(p => p.Themes).Distinct().ToArray();

        var themeRooms = themes.Where(p => p == theme || theme == null).Select(t => CreateThemeRoom(map, t)).ToArray();
        int index = 0;
        foreach (var room in themeRooms.WithNeighbors().Take(themeRooms.Length - 1))
        {
            if (++index < 25)
                room.Item3.Place().EastOf(room.Item2, gap: 64);
            else
                room.Item3.Place().SouthOf(room.Item2, gap: 64);

            var hall = StructureGenerator.AddStructure(room.Item2,
                    new Hall(Width: 128, room.Item2, room.Item3));

            map.Rooms.Add(hall);
        }

        var start = map.AddRoom(size: new Size(64, 64));
        start.Place().WestOf(themeRooms[0]);
        ThingPlacer.AddThing(ThingType.Player1_start, start, 0.5, 0.5, angle: Angle.East);

        var udmf = MapToUDMF(map);
        var expected = File.ReadAllText("Fixtures//all_textures_map.udmf");
        Assert.That(udmf, Is.EqualTo(expected));
    }

    /// <summary>
    /// Room showcasing all textures in this theme
    /// </summary>
    /// <param name="theme"></param>
    /// <returns></returns>
    private Room CreateThemeRoom(Map map, string theme)
    {
        var room = map.AddRoom(size: new Size(512, 512));
        room.Comment = theme;
        room.Ceiling = 256;
        room.LightLevel = 255;

        var textures = DoomConfig.DoomTextureInfo.Values.Where(p => p.Themes.Contains(theme)).ToArray();

        var texturePillars = textures.Select(texture =>
        {
            var pillar = room.AddInnerStructure(size: new Size(texture.Size!.Width, texture.Size!.Width));
            pillar.Ceiling = 0;
            pillar.LightLevel = 255;
            pillar.Floor = texture.Size!.Height;
            pillar.WallTexture = new TextureInfo(new TextureQuery(TextureName: texture.Name), DrawLowerFromBottom: true);

            pillar.LineSpecials[Side.Top] = new Plat_DownWaitUpStay(Tag: 0, Speed: Speed.StandardLift);
            pillar.LineSpecials[Side.Bottom] = new Plat_DownWaitUpStay(Tag: 0, Speed: Speed.StandardLift);
            pillar.LineSpecials[Side.Right] = new Plat_DownWaitUpStay(Tag: 0, Speed: Speed.StandardLift);
            pillar.LineSpecials[Side.Left] = new Plat_DownWaitUpStay(Tag: 0, Speed: Speed.StandardLift);

            return pillar;
        }).ToArray();

        if (texturePillars.Length > 1 && texturePillars.Length < 4)
            room.Size = new Size(320, 1024);

        PlaceTexturePillars(room, texturePillars);

        while (texturePillars.Any(p => texturePillars.Any(q => p != q && p.Bounds.IntersectsWith(q.Bounds))))
        {
            room.Size = new Size(room.Size.Width + 128, room.Size.Height + 128);
            PlaceTexturePillars(room, texturePillars);
        }

        return room;
    }

    private void PlaceTexturePillars(Room room, Room[] pillars)
    {
        if (pillars.Length > 12)
            pillars.Place().InGrid(room, 6, new Padding(64));
        else if (pillars.Length > 6)
            pillars.Place().InGrid(room, 4, new Padding(64));
        else if (pillars.Length >= 4)
            pillars.Place().InGrid(room, 2, new Padding(64));
        else if (pillars.Length == 1)
            pillars[0].Place().InCenterOf(room);
        else
            pillars.Place().InLine(room, Side.Bottom, 0.5, new Padding(64));
    }
}
