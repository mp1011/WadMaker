namespace WadMaker.Services;

public class RoomPositionResolver
{
    public void Execute(Map map)
    {
        List<Room> processedRooms = new List<Room>();

        foreach(var room in map.Rooms)
        {
            ResolveNeighborPositions(room, processedRooms);
        }
    }

    private void ResolveNeighborPositions(Room room, List<Room> processedRooms)
    {
        if(processedRooms.Contains(room))
            return;

        processedRooms.Add(room);

        foreach(var neighbor in room.RelatedRooms)
        {
            ResolveRoomRelation(room, neighbor);
        }

        foreach (var neighbor in room.RelatedRooms)
        {
            ResolveNeighborPositions(neighbor.Other, processedRooms);
        }
    }

    private void ResolveRoomRelation(Room room, RoomRelation relation)
    {
        relation.Other.Place().ToSideOf(room, relation.Side);
    }
}
