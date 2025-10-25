using System.Drawing;
using WadMaker.Extensions;
using WadMaker.Models;
using WadMaker.Models.Geometry;
using WadMaker.Models.Placement;
using WadMaker.Services;
using WadMaker.Tests.TestHelpers;

namespace WadMaker.Tests.Performance;

internal class StressTestMap : StandardTest
{
    public string Create(int size, int pillars)
    {
        var map = new Map();

        var room = map.AddRoom(size: new Size(128, 128));

        ThingPlacer.AddPlayerStartToFirstRoomCenter(map);

        var bigRoom = map.AddRoom(size: new Size(size, size));
        bigRoom.Ceiling = 256;

        bigRoom.Place().NorthOf(room);

        foreach (var x in Enumerable.Range(0, pillars))
        {
            var pillar = bigRoom.AddInnerStructure(size: new Size(128, 128));
            pillar.Ceiling = 0;
            pillar.Floor = 128;
            pillar.WallTexture = new TextureInfo(Texture.BFALL1);
        }

        bigRoom.InnerStructures.Place().InGrid(bigRoom, (int)Math.Sqrt(bigRoom.InnerStructures.Count()), new Padding(128));

        ThingPlacer.AddFormation(ThingType.Arachnotron, bigRoom, 400, Angle.North, ThingFlags.AllSkillsAndModes, new ThingPlacement(0.5,0.1), ThingPattern.Triangle, 128);
        var udmf = MapToUDMF(map);
        return udmf;
    }
}
