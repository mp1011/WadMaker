namespace WadMaker.Models;

/// <summary>
/// Order of rooms the player is able to visit.
/// </summary>
/// <param name="Nodes"></param>
public record PlayerPath(PlayerPathNode[] Nodes);

/// <summary>
/// <param name="RequiredRooms">Rooms the player needs to have been in before they can progress to the next Node</param>
/// <param name="OptionalRooms"></param>
public record PlayerPathNode(Room[] RequiredRooms, Room[] OptionalRooms)
{
}