using WadMaker;

var services = ServiceContainer.CreateServiceProvider(ServiceContainer.StandardDependencies);
var wadMaker = services.GetRequiredService<WadMakerMain>();

//Legacy.Flags = LegacyFlags.DontResolveCrossingLines;

var map = new Map();

var dorm = map.AddRoom(CreateDorm(map)); 

//var floor1 = CreateFloor1(map);


const int PillarDistance = 512;
const int PillarWidth = 256;

wadMaker.ThingPlacer.AddPlayerStartToFirstRoomCenter(map);


var udmf = wadMaker.MapToUdmf(map);
Console.WriteLine("Done");

static Room CreateFloor1(Map map)
{
    var floor = CreateFloor(map);
    var supportPillars = floor.Pillars.OrderBy(p=>p.Center.Y).ThenBy(p=>p.Center.X).ToArray();

    var divider1 = floor.AddPillar(new Size(PillarDistance, 32));
    divider1.Place().ToInsideOf(floor, Side.Left, targetAnchor: Anchor.Absolute(PillarDistance + PillarWidth / 2));

    var divider2 = floor.AddPillar(new Size(supportPillars[1].Bounds().X - supportPillars[0].Bounds().Right, 32));
    divider2.Place().ToInsideOf(floor, Side.Left, gap: -supportPillars[0].Bounds().Right, targetAnchor: Anchor.Absolute(PillarDistance + PillarWidth / 2));

    var divider3 = floor.AddPillar(new Size(supportPillars[2].Bounds().X - supportPillars[1].Bounds().Right, 32));
    divider3.Place().ToInsideOf(floor, Side.Left, gap: -supportPillars[1].Bounds().Right, targetAnchor: Anchor.Absolute(PillarDistance + PillarWidth / 2));

    var divider4 = floor.AddPillar(new Size(floor.Size.Width - supportPillars[2].Bounds().Right, 32));
    divider4.Place().ToInsideOf(floor, Side.Left, gap: -supportPillars[2].Bounds().Right, targetAnchor: Anchor.Absolute(PillarDistance + PillarWidth / 2));

   
    return floor;
}

Room CreateDorm(Map map)
{
    var dorms = new Room(map, size: new Size(PillarDistance * 2, PillarDistance));
    dorms.Floor = 0;
    dorms.Ceiling = 256;
    dorms.UpperLeft = new Point(PillarDistance + PillarWidth, -16);
    dorms.BottomRight = new Point(PillarWidth + 1524, -PillarDistance - 32);


    var beds = Enumerable.Range(0, 25).Select(p =>
    {
        // 75, 32
        var bed = dorms.AddInnerStructure(size: new Size(85, 42));      
        bed.Floor = 18;
        bed.Ceiling = 0;
        return bed;
    }).ToArray();

    beds.Place().InGrid(dorms, columns: 5, new Padding(32));


    foreach(var bed in beds)
    {
        wadMaker.ThingPlacer.AddThing(ThingType.Dead_player, bed, 0.5, 0.5, ThingFlags.AllSkillsAndModes);
    }

    return dorms;
}



static Room CreateFloor(Map map)
{
    var room = map.AddRoom(size: new Size(3840, 3840));
    room.Ceiling = 256;

    var pillars = Enumerable.Range(0, 9).Select(p => room.AddPillar(size: new Size(PillarWidth, PillarWidth))).ToArray();

    pillars.Place().InGrid(room, 3, new Padding(PillarDistance));

    return room;
}