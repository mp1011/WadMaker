namespace WadMaker.Models.Placement;

public record RoomRelation(Side Side, Room Other, Anchor Anchor, Anchor OtherAnchor, int Spacing);
