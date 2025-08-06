namespace WadMaker.Services;

public class ThingPlacer
{

  public void AddThing(ThingType thing, Room room, double pctX, double pctY)
  {
    room.Things.Add(new Thing(new thing(
           x: room.UpperLeft.X + room.Bounds.Width * pctX,
           y: room.UpperLeft.Y - room.Bounds.Height * pctY,
           angle: 0,
           type: (int)thing,
           skill1: true,
           skill2: true,
           skill3: true,
           skill4: true,
           skill5: true,
           single: true,
           ambush: null,
           dm: false,
           coop: false
       )));
  }

  public void AddPlayerStartToFirstRoomCenter(Map map)
  {
    AddThing(ThingType.Player1Start, map.Rooms.First(), 0.5, 0.5);
  }
}